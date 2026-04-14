using MediatR;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class LoginRequestHandler :
    IRequestHandler<SecurityServiceQueries.GetExternalProvidersQuery, Result<List<ExternalProviderDetails>>>,
    IRequestHandler<SecurityServiceCommands.LoginCommand, Result>
{
    private readonly SignInManager<ApplicationUser> SignInManager;

    public LoginRequestHandler(SignInManager<ApplicationUser> signInManager)
    {
        this.SignInManager = signInManager;
    }

    public async Task<Result<List<ExternalProviderDetails>>> Handle(SecurityServiceQueries.GetExternalProvidersQuery query, CancellationToken cancellationToken)
    {
        var providers = (await this.SignInManager.GetExternalAuthenticationSchemesAsync())
            .Select(scheme => new ExternalProviderDetails(scheme.Name, scheme.DisplayName ?? scheme.Name))
            .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Success(providers);
    }

    public async Task<Result> Handle(SecurityServiceCommands.LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await this.SignInManager.PasswordSignInAsync(command.Username, command.Password, command.RememberLogin, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return Result.Success();
        }

        if (result.IsLockedOut)
        {
            return Result.Failure("Your account has been locked. Please try again later or contact support.");
        }

        if (result.IsNotAllowed)
        {
            return Result.Failure("You are not allowed to sign in. Please confirm your email address.");
        }

        if (result.RequiresTwoFactor)
        {
            return Result.Failure("Two-factor authentication is required.");
        }

        return Result.Failure("Invalid username or password.");
    }
}
