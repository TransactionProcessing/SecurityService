using MediatR;
using Microsoft.AspNetCore.Authentication;
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
    private readonly MfaEnrollmentTransactionProtector EnrollmentTransactionProtector;

    public LoginRequestHandler(SignInManager<ApplicationUser> signInManager,
                               UserManager<ApplicationUser> userManager,
                               IMfaPolicyService mfaPolicyService,
                               MfaAccountService mfaAccountService,
                               MfaSignInTransactionProtector transactionProtector,
                               MfaEnrollmentTransactionProtector enrollmentTransactionProtector) {
        this.SignInManager = signInManager;
        this.UserManager = userManager;
        this.MfaPolicyService = mfaPolicyService;
        this.MfaAccountService = mfaAccountService;
        this.TransactionProtector = transactionProtector;
        this.EnrollmentTransactionProtector = enrollmentTransactionProtector;
    }

    public async Task<Result<List<ExternalProviderDetails>>> Handle(SecurityServiceQueries.GetExternalProvidersQuery query, CancellationToken cancellationToken) {
        List<ExternalProviderDetails> providers = (await this.SignInManager.GetExternalAuthenticationSchemesAsync())
                                                  .Select(scheme => new ExternalProviderDetails(scheme.Name, scheme.DisplayName ?? scheme.Name))
                                                  .OrderBy(provider => provider.DisplayName, StringComparer.OrdinalIgnoreCase)
                                                  .ToList();

        return Result.Success(providers);
    }

    public async Task<Result> Handle(SecurityServiceCommands.LoginCommand command, CancellationToken cancellationToken) {
        SignInResult result = await this.SignInManager.PasswordSignInAsync(command.Username, command.Password, command.RememberLogin, lockoutOnFailure: true);

        if (result.Succeeded) {
            return Result.Success();
        }

        if (result.IsLockedOut) {
            return Result.Failure(AuthenticationErrors.AccountLocked);
        }

        if (result.IsNotAllowed) {
            return Result.Failure(AuthenticationErrors.EmailNotConfirmed);
        }

        if (result.RequiresTwoFactor) {
            return Result.Failure(AuthenticationErrors.TwoFactorRequired);
        }

        return Result.Failure(AuthenticationErrors.InvalidUserNameOrPassword);
    }

    public class AuthenticationErrors {
        public const String InvalidUserNameOrPassword = "Invalid username or password.";
        public const String AccountLocked = "Your account has been locked. Please try again later or contact support";
        public const String EmailNotConfirmed = "You are not allowed to sign in. Please confirm your email address.";
        public const String TwoFactorRequired = "Two-factor authentication is required.";

        public const String MFAExpired = "The MFA sign-in request has expired. Start signing in again.";
        public const String MFANoLongerValid = "The MFA sign-in request is no longer valid.";
        public const String AuthenticatorOrRecoverCodeInvalid = "The authenticator or recovery code was invalid.";
    }

    public async Task<Result<LoginOutcome>> Handle(SecurityServiceCommands.BeginLoginCommand command, CancellationToken cancellationToken) {
        ApplicationUser? user = await this.UserManager.FindByNameAsync(command.Username);
        if (user is null) {
            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.InvalidUserNameOrPassword));
        }

        Boolean policyRequiresMfa = await this.MfaPolicyService.IsMfaRequiredAsync(user, cancellationToken);
        Boolean mfaEnabled = await this.UserManager.GetTwoFactorEnabledAsync(user);

        if (mfaEnabled) {
            SignInResult passwordResult = await this.SignInManager.PasswordSignInAsync(user, command.Password, command.RememberLogin, lockoutOnFailure: true);
            if (passwordResult.Succeeded) {
                return Result.Success<LoginOutcome>(new LoginCompleted(command.ReturnUrl));
            }

            if (passwordResult.IsLockedOut) {
                return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.AccountLocked));
            }

            if (passwordResult.IsNotAllowed) {
                return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.EmailNotConfirmed));
            }

            if (passwordResult.RequiresTwoFactor) {
                String transaction = this.TransactionProtector.Protect(user.Id, command.ReturnUrl, command.RememberLogin);
                return Result.Success<LoginOutcome>(new LoginRequiresMfa(transaction));
            }

            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.InvalidUserNameOrPassword));
        }

        SignInResult passwordCheck = await this.SignInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (passwordCheck.Succeeded == false) {
            if (passwordCheck.IsLockedOut) {
                return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.AccountLocked));
            }

            if (passwordCheck.IsNotAllowed) {
                return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.EmailNotConfirmed));
            }

            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.InvalidUserNameOrPassword));
        }

        if (policyRequiresMfa) {
            String transaction = this.EnrollmentTransactionProtector.Protect(user.Id, command.ReturnUrl);
            return Result.Success<LoginOutcome>(new LoginRequiresMfaEnrollment(transaction));
        }

        await this.SignInManager.SignInAsync(user, command.RememberLogin);
        return Result.Success<LoginOutcome>(new LoginCompleted(command.ReturnUrl));
    }

    public async Task<Result<LoginOutcome>> Handle(SecurityServiceCommands.CompleteMfaLoginCommand command, CancellationToken cancellationToken) {
        if (this.TransactionProtector.TryUnprotect(command.Transaction, out MfaSignInTransaction? transaction) == false || transaction is null) {
            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.MFAExpired));
        }

        ApplicationUser? user = await this.SignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null || !string.Equals(user.Id, transaction.UserId, StringComparison.Ordinal) || await this.UserManager.GetTwoFactorEnabledAsync(user) == false || await this.UserManager.IsLockedOutAsync(user)) {
            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.MFANoLongerValid));
        }

        Boolean valid = await this.UserManager.VerifyTwoFactorTokenAsync(user, "Authenticator", command.Code);
        if (valid == false) {
            valid = await this.MfaAccountService.RedeemRecoveryCodeAsync(user, command.Code, cancellationToken);
        }

        if (valid == false) {
            await this.UserManager.AccessFailedAsync(user);
            return Result.Success<LoginOutcome>(new LoginRejected(AuthenticationErrors.AuthenticatorOrRecoverCodeInvalid));
        }

        await this.UserManager.ResetAccessFailedCountAsync(user);
        await this.SignInManager.Context.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
        await this.SignInManager.SignInAsync(user, transaction.RememberLogin);
        return Result.Success<LoginOutcome>(new LoginCompleted(transaction.ReturnUrl));
    }
}
