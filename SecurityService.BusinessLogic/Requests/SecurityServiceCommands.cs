using System;
using System.Collections.Generic;
using MediatR;
using SecurityService.Models;
using SimpleResults;

namespace SecurityService.BusinessLogic.Requests;

public record SecurityServiceCommands {
    public record CreateApiResourceCommand(string Name, string DisplayName, string Description, string Secret, List<string> Scopes, List<string> UserClaims) : IRequest<Result>;
    public record ChangeUserPasswordCommand(String UserName,
                                     String CurrentPassword,
                                     String NewPassword,
                                     String ClientId) : IRequest<Result<ChangeUserPasswordResult>>;

    public record ConfirmUserEmailAddressCommand(String UserName, String ConfirmEmailToken) : IRequest<Result>;

    public record CreateApiScopeCommand(String Name, String DisplayName, String Description) : IRequest<Result>;

    public record CreateClientCommand(String ClientId,
                                      String Secret,
                                      String ClientName,
                                      String ClientDescription,
                                      List<String> AllowedScopes,
                                      List<String> AllowedGrantTypes,
                                      String ClientUri,
                                      List<String> ClientRedirectUris,
                                      List<String> ClientPostLogoutRedirectUris,
                                      Boolean RequireConsent,
                                      Boolean AllowOfflineAccess) : IRequest<Result>;

    public record CreateIdentityResourceCommand(String Name,
                                                String DisplayName,
                                                String Description,
                                                Boolean Required,
                                                Boolean Emphasize,
                                                Boolean ShowInDiscoveryDocument,
                                                List<String> Claims) : IRequest<Result>;

    public record CreateRoleCommand(Guid RoleId, String Name) : IRequest<Result>;

    public record CreateUserCommand(Guid UserId,
                                    String GivenName,
                                    String MiddleName,
                                    String FamilyName,
                                    String UserName,
                                    String Password,
                                    String EmailAddress,
                                    String PhoneNumber,
                                    Dictionary<String, String> Claims,
                                    List<String> Roles) : IRequest<Result>;

    public record ProcessPasswordResetConfirmationCommand(String Username,
                                                          String Token,
                                                          String Password,
                                                          String ClientId) : IRequest<Result<String>>;

    public record ProcessPasswordResetRequestCommand(String Username,
                                              String EmailAddress,
                                              String ClientId) : IRequest<Result>;

    public record SendWelcomeEmailCommand(String Username) : IRequest<Result>;

}