using MediatR;
using Microsoft.AspNetCore.Identity;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.BusinessLogic.Requests;
using SecurityService.Database.Entities;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.RequestHandlers;

public sealed class LoginRequestHandler :
    IRequestHandler<SecurityServiceQueries.GetExternalProvidersQuery, Result<List<ExternalProviderDetails>>>,
    IRequestHandler<SecurityServiceCommands.LoginCommand, Result>,
    IRequestHandler<SecurityServiceCommands.BeginLoginCommand, Result<LoginOutcome>>,
    IRequestHandler<SecurityServiceCommands.CompleteMfaLoginCommand, Result<LoginOutcome>>
{
    private readonly SignInManager<ApplicationUser> SignInManager;
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly IMfaPolicyService MfaPolicyService;
    private readonly MfaAccountService MfaAccountService;
    private readonly MfaSignInTransactionProtector TransactionProtector;

    public LoginRequestHandler(SignInManager<ApplicationUser> signInManager,
                               UserManager<ApplicationUser> userManager,
                               IMfaPolicyService mfaPolicyService,
                               MfaAccountService mfaAccountService,
                               MfaSignInTransactionProtector transactionProtector)
    {
        this.SignInManager = signInManager;
        this.UserManager = userManager;
        this.MfaPolicyService = mfaPolicyService;
        this.MfaAccountService = mfaAccountService;
        this.TransactionProtector = transactionProtector;
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

    public async Task<Result<LoginOutcome>> Handle(SecurityServiceCommands.BeginLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await this.UserManager.FindByNameAsync(command.Username);
        if (user is null)
        {
            return Result.Success<LoginOutcome>(new LoginRejected("Invalid username or password."));
        }

        var policyRequiresMfa = await this.MfaPolicyService.IsMfaRequiredAsync(user, cancellationToken);
        var mfaEnabled = await this.UserManager.GetTwoFactorEnabledAsync(user);
        if (policyRequiresMfa && mfaEnabled == false)
        {
            return Result.Success<LoginOutcome>(new LoginRejected("MFA enrollment is required before you can sign in."));
        }

        var passwordResult = await this.SignInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (passwordResult.Succeeded == false)
        {
            if (passwordResult.IsLockedOut)
            {
                return Result.Success<LoginOutcome>(new LoginRejected("Your account has been locked. Please try again later or contact support."));
            }

            if (passwordResult.IsNotAllowed)
            {
                return Result.Success<LoginOutcome>(new LoginRejected("You are not allowed to sign in. Please confirm your email address."));
            }

            return Result.Success<LoginOutcome>(new LoginRejected("Invalid username or password."));
        }

        if (mfaEnabled)
        {
            var transaction = this.TransactionProtector.Protect(user.Id, command.ReturnUrl, command.RememberLogin);
            return Result.Success<LoginOutcome>(new LoginRequiresMfa(transaction));
        }

        await this.SignInManager.SignInAsync(user, command.RememberLogin);
        return Result.Success<LoginOutcome>(new LoginCompleted(command.ReturnUrl));
    }

    public async Task<Result<LoginOutcome>> Handle(SecurityServiceCommands.CompleteMfaLoginCommand command, CancellationToken cancellationToken)
    {
        if (this.TransactionProtector.TryUnprotect(command.Transaction, out var transaction) == false || transaction is null)
        {
            return Result.Success<LoginOutcome>(new LoginRejected("The MFA sign-in request has expired. Start signing in again."));
        }

        var user = await this.UserManager.FindByIdAsync(transaction.UserId);
        if (user is null || await this.UserManager.GetTwoFactorEnabledAsync(user) == false || await this.UserManager.IsLockedOutAsync(user))
        {
            return Result.Success<LoginOutcome>(new LoginRejected("The MFA sign-in request is no longer valid."));
        }

        var valid = await this.UserManager.VerifyTwoFactorTokenAsync(user, "Authenticator", command.Code)
                    || await this.MfaAccountService.RedeemRecoveryCodeAsync(user, command.Code, cancellationToken);
        if (valid == false)
        {
            await this.UserManager.AccessFailedAsync(user);
            return Result.Success<LoginOutcome>(new LoginRejected("The authenticator or recovery code was invalid."));
        }

        await this.SignInManager.SignInAsync(user, transaction.RememberLogin);
        return Result.Success<LoginOutcome>(new LoginCompleted(transaction.ReturnUrl));
    }
}
