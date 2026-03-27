using MediatR;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.Requests;

public static class SecurityServiceCommands
{
    public sealed record CreateClientCommand(
        string ClientId,
        string? Secret,
        string ClientName,
        string? ClientDescription,
        IReadOnlyCollection<string> AllowedScopes,
        IReadOnlyCollection<string> AllowedGrantTypes,
        string? ClientUri,
        IReadOnlyCollection<string> ClientRedirectUris,
        IReadOnlyCollection<string> ClientPostLogoutRedirectUris,
        bool RequireConsent,
        bool AllowOfflineAccess) : IRequest<Result>;

    public sealed record CreateApiScopeCommand(string Name, string? DisplayName, string? Description)
        : IRequest<Result>;

    public sealed record CreateApiResourceCommand(
        string Name,
        string? DisplayName,
        string? Description,
        string? Secret,
        IReadOnlyCollection<string> Scopes,
        IReadOnlyCollection<string> UserClaims) : IRequest<Result>;

    public sealed record CreateIdentityResourceCommand(
        string Name,
        string? DisplayName,
        string? Description,
        bool Required,
        bool Emphasize,
        bool ShowInDiscoveryDocument,
        IReadOnlyCollection<string> Claims) : IRequest<Result>;

    public sealed record CreateRoleCommand(string Name) : IRequest<Result>;

    public sealed record CreateUserCommand(
        string? GivenName,
        string? MiddleName,
        string? FamilyName,
        string UserName,
        string Password,
        string? EmailAddress,
        string? PhoneNumber,
        IReadOnlyDictionary<string, string> Claims,
        List<string> Roles) : IRequest<Result>;

    public record ChangeUserPasswordCommand(String UserName,
                                            String CurrentPassword,
                                            String NewPassword,
                                            String ClientId) : IRequest<Result<ChangeUserPasswordResult>>;

    public record ConfirmUserEmailAddressCommand(String UserName, String ConfirmEmailToken) : IRequest<Result>;

    public record SendWelcomeEmailCommand(String Username) : IRequest<Result>;
    public record ProcessPasswordResetRequestCommand(String Username,
                                                     String EmailAddress,
                                                     String ClientId) : IRequest<Result>;
    public record ProcessPasswordResetConfirmationCommand(String Username,
                                                          String Token,
                                                          String Password,
                                                          String ClientId) : IRequest<Result<String>>;
}
