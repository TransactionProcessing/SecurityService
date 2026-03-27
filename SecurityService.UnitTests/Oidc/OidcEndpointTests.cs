using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenIddict.Validation.AspNetCore;
using SecurityService.Database;
using SecurityService.Database.DbContexts;
using SecurityService.Database.Entities;
using SecurityService.Oidc;
using SecurityService.UnitTests.Infrastructure;
using Shouldly;

namespace SecurityService.UnitTests.Oidc;

public class OidcEndpointTests
{
    [Fact]
    public async Task UserInfoAsync_WithValidPrincipal_ReturnsJsonPayload()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true,
            GivenName = "Alice",
            FamilyName = "Example"
        };
        userManager.Setup(manager => manager.FindByIdAsync("user-1")).ReturnsAsync(user);
        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync(["Admin"]);
        userManager.Setup(manager => manager.GetClaimsAsync(user)).ReturnsAsync([]);

        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(OpenIddict.Abstractions.OpenIddictConstants.Claims.Subject, "user-1"));
        identity.SetScopes(OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Roles);
        var principal = new ClaimsPrincipal(identity);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IAuthenticationService>(new FakeAuthenticationService(AuthenticateResult.Success(new AuthenticationTicket(principal, OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme))));

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var result = await OidcEndpoints.UserInfoAsync(context, userManager.Object);
        await result.ExecuteAsync(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        context.Response.Body.Position = 0;
        var payload = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        payload.ShouldContain("\"sub\":\"user-1\"");
        payload.ShouldContain("\"email\":\"alice@example.com\"");
    }

    [Fact]
    public async Task TokenAsync_ClientCredentialsWithoutRequestedScopes_FallsBackToConfiguredClientScopes()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(this.TokenAsync_ClientCredentialsWithoutRequestedScopes_FallsBackToConfiguredClientScopes));
        using var scope = provider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SecurityServiceDbContext>();

        dbContext.ClientDefinitions.Add(new ClientDefinition
        {
            Id = Guid.NewGuid(),
            ClientId = "serviceClient",
            ClientName = "Service Client",
            AllowedScopesJson = JsonListSerializer.Serialize(["transactionProcessor"]),
            AllowedGrantTypesJson = JsonListSerializer.Serialize([OpenIddictConstants.GrantTypes.ClientCredentials]),
            RedirectUrisJson = "[]",
            PostLogoutRedirectUrisJson = "[]",
            ClientType = "confidential"
        });

        await dbContext.SaveChangesAsync();

        var request = new OpenIddictRequest
        {
            GrantType = OpenIddictConstants.GrantTypes.ClientCredentials,
            ClientId = "serviceClient"
        };

        var scopes = await OidcEndpoints.ResolveClientCredentialsScopesAsync(
            request,
            dbContext,
            CancellationToken.None);

        scopes.ShouldContain("transactionProcessor");
    }

    [Fact]
    public async Task CreatePrincipalAsync_WhenUserClaimsDuplicateBuiltInClaims_DoesNotDuplicateValues()
    {
        var userManager = IdentityMocks.CreateUserManager();
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "merchantuser@stagingmerchant1.co.uk",
            Email = "merchantuser@stagingmerchant1.co.uk",
            GivenName = "Staging Merchant 1",
            FamilyName = "User"
        };

        userManager.Setup(manager => manager.GetRolesAsync(user)).ReturnsAsync(["Merchant"]);
        userManager.Setup(manager => manager.GetClaimsAsync(user)).ReturnsAsync(
        [
            new Claim(OpenIddictConstants.Claims.Email, "merchantuser@stagingmerchant1.co.uk"),
            new Claim(OpenIddictConstants.Claims.GivenName, "Staging Merchant 1"),
            new Claim(OpenIddictConstants.Claims.FamilyName, "User"),
            new Claim(OpenIddictConstants.Claims.Role, "Merchant")
        ]);

        var principal = await OidcHelpers.CreatePrincipalAsync(
            user,
            userManager.Object,
            [OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles],
            ["transactionProcessor"],
            authorizationId: null);

        principal.Claims.Count(claim => claim.Type == OpenIddictConstants.Claims.Email && claim.Value == "merchantuser@stagingmerchant1.co.uk").ShouldBe(1);
        principal.Claims.Count(claim => claim.Type == OpenIddictConstants.Claims.GivenName && claim.Value == "Staging Merchant 1").ShouldBe(1);
        principal.Claims.Count(claim => claim.Type == OpenIddictConstants.Claims.FamilyName && claim.Value == "User").ShouldBe(1);
        principal.Claims.Count(claim => claim.Type == OpenIddictConstants.Claims.Role && claim.Value == "Merchant").ShouldBe(1);
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        private readonly AuthenticateResult _authenticateResult;

        public FakeAuthenticationService(AuthenticateResult authenticateResult)
        {
            this._authenticateResult = authenticateResult;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(this._authenticateResult);

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }
}
