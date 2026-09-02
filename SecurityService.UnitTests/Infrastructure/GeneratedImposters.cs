using Imposter.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MediatR;
using OpenIddict.Abstractions;
using SecurityService.Database.Entities;

[assembly: GenerateImposter(typeof(IMediator))]
[assembly: GenerateImposter(typeof(IOpenIddictApplicationManager))]
[assembly: GenerateImposter(typeof(IOpenIddictAuthorizationManager))]
[assembly: GenerateImposter(typeof(IOpenIddictScopeManager))]
[assembly: GenerateImposter(typeof(IUserPasswordStore<ApplicationUser>))]
[assembly: GenerateImposter(typeof(IPasswordHasher<ApplicationUser>))]
[assembly: GenerateImposter(typeof(ILogger<>))]
[assembly: GenerateImposter(typeof(IHttpContextAccessor))]
[assembly: GenerateImposter(typeof(IUserClaimsPrincipalFactory<ApplicationUser>))]
[assembly: GenerateImposter(typeof(IAuthenticationSchemeProvider))]
[assembly: GenerateImposter(typeof(IUserConfirmation<ApplicationUser>))]








