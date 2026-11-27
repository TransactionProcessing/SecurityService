using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using TransactionProcessing.CertificateDefaults;

var builder = WebApplication.CreateBuilder(args);

Console.Title = "Security Service Test UI";

var hostingConfiguration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("hosting.json", optional: true)
    .AddJsonFile("hosting.development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
builder.WebHost.UseConfiguration(hostingConfiguration);

builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var authority = builder.Configuration.GetValue<string>("AppSettings:Authority");
var clientId = builder.Configuration.GetValue<string>("AppSettings:ClientId");
var clientSecret = builder.Configuration.GetValue<string>("AppSettings:ClientSecret");

Console.WriteLine($"Authority is {authority}");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("Cookies")
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
        };

        options.ClientSecret = clientSecret;
        options.ClientId = clientId;
        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
        options.ResponseType = "code";
        options.UsePkce = true;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");

        options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.IssuerAddress = $"{authority}/connect/authorize";
            return Task.CompletedTask;
        };
    });

builder.WebHost.ConfigureTransactionProcessingHttps();

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapHealthChecks("health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = TransactionProcessing.Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
});
app.MapHealthChecks("healthui", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
