using MediatR;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class LoginRequestHandler :
    IRequestHandler<SecurityServiceQueries.GetExternalProvidersQuery, Result<List<ExternalProviderDetails>>>,
    IRequestHandler<SecurityServiceCommands.LoginCommand, Result>
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginRequestHandler(SignInManager<ApplicationUser> signInManager)
    {
        this._signInManager = signInManager;
    }

    public async Task<Result<List<ExternalProviderDetails>>> Handle(SecurityServiceQueries.GetExternalProvidersQuery query, CancellationToken cancellationToken)
    {
        var providers = (await this._signInManager.GetExternalAuthenticationSchemesAsync())
            .Select(scheme => new ExternalProviderDetails(scheme.Name, scheme.DisplayName ?? scheme.Name))
            .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Success(providers);
    }

    public async Task<Result> Handle(SecurityServiceCommands.LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await this._signInManager.PasswordSignInAsync(command.Username, command.Password, command.RememberLogin, lockoutOnFailure: true);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure("Invalid username or password.");
    }
}
