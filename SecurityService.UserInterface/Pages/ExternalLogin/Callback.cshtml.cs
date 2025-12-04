using System.Security.Authentication;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using SecurityService.UserInterface.Common;

namespace IdentityServerHost.Pages.ExternalLogin;

using Microsoft.Extensions.Logging;
using SecurityService.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _logger = logger;
        _events = events;
    }

    public async Task<IActionResult> OnGet()
    {
        var result = await AuthenticateExternalAsync();

        LogExternalClaims(result.Principal);

        var (provider, providerUserId) = GetExternalProviderInfo(result);
        var user = await FindOrProvisionUserAsync(provider, providerUserId, result.Principal.Claims);

        var (claims, props) = CaptureExternalContext(result);
        await SignInUserAsync(user, claims, props);

        await ClearTemporaryExternalCookieAsync();

        var returnUrl = GetReturnUrl(result);
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

        await RaiseLoginSuccessEvent(provider, providerUserId, user, context);

        return context?.IsNativeClient() == true
            ? this.LoadingPage(returnUrl)
            : Redirect(returnUrl);
    }

    private async Task<AuthenticateResult> AuthenticateExternalAsync()
    {
        var result = await HttpContext.AuthenticateAsync(
            IdentityServerConstants.ExternalCookieAuthenticationScheme);

        if (result?.Succeeded != true)
            throw new AuthenticationException("External authentication error");

        return result;
    }

    private void LogExternalClaims(ClaimsPrincipal externalUser)
    {
        if (!_logger.IsEnabled(LogLevel.Debug)) return;

        var claims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
        _logger.LogDebug("External claims: {@claims}", claims);
    }

    private (string provider, string providerUserId) GetExternalProviderInfo(AuthenticateResult result)
    {
        var externalUser = result.Principal;

        var userIdClaim =
            externalUser.FindFirst(JwtClaimTypes.Subject) ??
            externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
            throw new AuthenticationException("Unknown userid");

        var provider = result.Properties.Items["scheme"];
        var providerUserId = userIdClaim.Value;

        return (provider, providerUserId);
    }

    private async Task<ApplicationUser> FindOrProvisionUserAsync(
        string provider, string providerUserId, IEnumerable<Claim> claims)
    {
        var user = await _userManager.FindByLoginAsync(provider, providerUserId);

        return user ?? await AutoProvisionUserAsync(provider, providerUserId, claims);
    }

    private (List<Claim> claims, AuthenticationProperties props)
        CaptureExternalContext(AuthenticateResult result)
    {
        var additionalClaims = new List<Claim>();
        var props = new AuthenticationProperties();

        CaptureExternalLoginContext(result, additionalClaims, props);

        return (additionalClaims, props);
    }

    private Task SignInUserAsync(
        ApplicationUser user,
        List<Claim> claims,
        AuthenticationProperties props)
    {
        return _signInManager.SignInWithClaimsAsync(user, props, claims);
    }

    private Task RaiseLoginSuccessEvent(
        string provider,
        string providerUserId,
        ApplicationUser user,
        AuthorizationRequest context)
    {
        return _events.RaiseAsync(
            new UserLoginSuccessEvent(
                provider,
                providerUserId,
                user.Id,
                user.UserName,
                true,
                context?.Client.ClientId));
    }


    private static string GetReturnUrl(AuthenticateResult result)
    {
        return result.Properties.Items["returnUrl"] ?? "~/";
    }


    private Task ClearTemporaryExternalCookieAsync()
    {
        return HttpContext.SignOutAsync(
            IdentityServerConstants.ExternalCookieAuthenticationScheme);
    }


    private static async Task EnsureSucceededAsync(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new AuthenticationException(result.Errors.First().Description);
    }

    private async Task<ApplicationUser> AutoProvisionUserAsync(
        string provider,
        string providerUserId,
        IEnumerable<Claim> claims)
    {
        string sub = Guid.NewGuid().ToString();

        var user = new ApplicationUser
        {
            Id = sub,
            UserName = sub,
            Email = claims.GetClaimValue(JwtClaimTypes.Email, ClaimTypes.Email)
        };

        var nameClaims = claims.BuildDisplayNameClaims();

        await EnsureSucceededAsync(await _userManager.CreateAsync(user));

        if (nameClaims.Any())
        {
            await EnsureSucceededAsync(await _userManager
                .AddClaimsAsync(user, nameClaims));
        }

        await EnsureSucceededAsync(
            await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider)));

        return user;
    }


    //private async Task<ApplicationUser> AutoProvisionUserAsync(string provider, string providerUserId, IEnumerable<Claim> claims)
    //{
    //    var sub = Guid.NewGuid().ToString();

    //    var user = new ApplicationUser()
    //    {
    //        Id = sub,
    //        UserName = sub, // don't need a username, since the user will be using an external provider to login
    //    };

    //    // email
    //    var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
    //                claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
    //    if (email != null)
    //    {
    //        user.Email = email;
    //    }

    //    // create a list of claims that we want to transfer into our store
    //    var filtered = new List<Claim>();

    //    // user's display name
    //    var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
    //               claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
    //    if (name != null)
    //    {
    //        filtered.Add(new Claim(JwtClaimTypes.Name, name));
    //    }
    //    else
    //    {
    //        var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
    //                    claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
    //        var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
    //                   claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
    //        if (first != null && last != null)
    //        {
    //            filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
    //        }
    //        else if (first != null)
    //        {
    //            filtered.Add(new Claim(JwtClaimTypes.Name, first));
    //        }
    //        else if (last != null)
    //        {
    //            filtered.Add(new Claim(JwtClaimTypes.Name, last));
    //        }
    //    }

    //    var identityResult = await _userManager.CreateAsync(user);
    //    if (!identityResult.Succeeded) throw new AuthenticationException(identityResult.Errors.First().Description);

    //    if (filtered.Any())
    //    {
    //        identityResult = await _userManager.AddClaimsAsync(user, filtered);
    //        if (!identityResult.Succeeded) throw new AuthenticationException(identityResult.Errors.First().Description);
    //    }

    //    identityResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerUserId, provider));
    //    if (!identityResult.Succeeded) throw new AuthenticationException(identityResult.Errors.First().Description);

    //    return user;
    //}

    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // capture the idp used to login, so the session knows where the user came from
        localClaims.Add(new Claim(JwtClaimTypes.IdentityProvider, externalResult.Properties.Items["scheme"]));

        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}