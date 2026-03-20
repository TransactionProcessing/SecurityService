namespace SecurityService.BusinessLogic.IdentityManagement;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Requests;
using SecurityService.Models;
using Shared.Logger;
using Shared.Results;
using SimpleResults;

public class KeycloakIdentityManagementService : IIdentityManagementService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    private readonly HttpClient HttpClient;
    private readonly ServiceOptions ServiceOptions;
    private KeycloakTokenResponse TokenResponse;

    public KeycloakIdentityManagementService(ServiceOptions serviceOptions) : this(serviceOptions, new HttpClient())
    {
    }

    public KeycloakIdentityManagementService(ServiceOptions serviceOptions, HttpClient httpClient)
    {
        this.ServiceOptions = serviceOptions;
        this.HttpClient = httpClient;
    }

    public Task<Result> CreateApiResource(SecurityServiceCommands.CreateApiResourceCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported("API resource"));

    public Task<Result<ApiResource>> GetApiResource(SecurityServiceQueries.GetApiResourceQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<ApiResource>("API resource"));

    public Task<Result<List<ApiResource>>> GetApiResources(SecurityServiceQueries.GetApiResourcesQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<List<ApiResource>>("API resource"));

    public Task<Result> CreateApiScope(SecurityServiceCommands.CreateApiScopeCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported("API scope"));

    public Task<Result<ApiScope>> GetApiScope(SecurityServiceQueries.GetApiScopeQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<ApiScope>("API scope"));

    public Task<Result<List<ApiScope>>> GetApiScopes(SecurityServiceQueries.GetApiScopesQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<List<ApiScope>>("API scope"));

    public async Task<Result> CreateClient(SecurityServiceCommands.CreateClientCommand command, CancellationToken cancellationToken)
    {
        Result validationResult = ValidateGrantTypes(command.AllowedGrantTypes);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        KeycloakClientProviderSettings providerSettings = GetProviderSettings<KeycloakClientProviderSettings>(command.ProviderSettings);
        String realm = this.GetRealm(providerSettings.Realm);
        if (String.IsNullOrWhiteSpace(realm))
        {
            return Result.Invalid("No Keycloak realm is configured for client management");
        }

        KeycloakClientRepresentation request = new KeycloakClientRepresentation
                                               {
                                                   AdminUrl = command.ClientUri,
                                                   BaseUrl = command.ClientUri,
                                                   ClientId = command.ClientId,
                                                   Description = command.ClientDescription,
                                                   DirectAccessGrantsEnabled = providerSettings.DirectAccessGrantsEnabled ?? command.AllowedGrantTypes?.Contains(GrantType.ResourceOwnerPassword) == true,
                                                   FrontchannelLogout = providerSettings.FrontchannelLogout ?? false,
                                                   ImplicitFlowEnabled = providerSettings.ImplicitFlowEnabled ?? command.AllowedGrantTypes?.Contains(GrantType.Implicit) == true,
                                                   Name = command.ClientName,
                                                   PublicClient = providerSettings.PublicClient ?? String.Equals(providerSettings.ClientType, "public", StringComparison.OrdinalIgnoreCase),
                                                   RedirectUris = command.ClientRedirectUris ?? new List<String>(),
                                                   RootUrl = command.ClientUri,
                                                   Secret = command.Secret,
                                                   ServiceAccountsEnabled = providerSettings.ServiceAccountsEnabled ?? command.AllowedGrantTypes?.Contains(GrantType.ClientCredentials) == true,
                                                   StandardFlowEnabled = providerSettings.StandardFlowEnabled ?? command.AllowedGrantTypes?.Any(gt => gt == GrantType.AuthorizationCode || gt == GrantType.Hybrid) == true,
                                                   WebOrigins = providerSettings.WebOrigins ?? new List<String>()
                                               };

        if (command.ClientPostLogoutRedirectUris != null && command.ClientPostLogoutRedirectUris.Any())
        {
            request.Attributes["post.logout.redirect.uris"] = String.Join("##", command.ClientPostLogoutRedirectUris);
        }

        return await this.SendWithoutResponse(HttpMethod.Post, $"/admin/realms/{realm}/clients", request, cancellationToken);
    }

    public async Task<Result<Client>> GetClient(SecurityServiceQueries.GetClientQuery query, CancellationToken cancellationToken)
    {
        List<KeycloakClientRepresentation> clients = await this.GetClientsInternal(this.GetRealm(null), query.ClientId, cancellationToken);
        KeycloakClientRepresentation client = clients.SingleOrDefault();

        if (client == null)
        {
            return Result.NotFound($"No client found with Client Id [{query.ClientId}]");
        }

        return Result.Success(MapClient(client));
    }

    public async Task<Result<List<Client>>> GetClients(SecurityServiceQueries.GetClientsQuery query, CancellationToken cancellationToken)
    {
        List<KeycloakClientRepresentation> clients = await this.GetClientsInternal(this.GetRealm(null), null, cancellationToken);
        return Result.Success(clients.Select(MapClient).ToList());
    }

    public Task<Result> CreateIdentityResource(SecurityServiceCommands.CreateIdentityResourceCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported("identity resource"));

    public Task<Result<IdentityResource>> GetIdentityResource(SecurityServiceQueries.GetIdentityResourceQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<IdentityResource>("identity resource"));

    public Task<Result<List<IdentityResource>>> GetIdentityResources(SecurityServiceQueries.GetIdentityResourcesQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<List<IdentityResource>>("identity resource"));

    public async Task<Result> CreateRole(SecurityServiceCommands.CreateRoleCommand command, CancellationToken cancellationToken)
    {
        KeycloakRoleRepresentation request = new KeycloakRoleRepresentation
                                             {
                                                 Id = command.RoleId.ToString(),
                                                 Name = command.Name
                                             };

        return await this.SendWithoutResponse(HttpMethod.Post, $"/admin/realms/{this.GetRealm(null)}/roles", request, cancellationToken, HttpStatusCode.Created, HttpStatusCode.NoContent);
    }

    public async Task<Result<RoleDetails>> GetRole(SecurityServiceQueries.GetRoleQuery query, CancellationToken cancellationToken)
    {
        Result<KeycloakRoleRepresentation> result = await this.SendForResponse<KeycloakRoleRepresentation>(HttpMethod.Get,
            $"/admin/realms/{this.GetRealm(null)}/roles-by-id/{query.RoleId}", null, cancellationToken);

        if (result.IsFailed)
        {
            return ResultHelpers.CreateFailure(result);
        }

        return Result.Success(MapRole(result.Data));
    }

    public async Task<Result<List<RoleDetails>>> GetRoles(SecurityServiceQueries.GetRolesQuery query, CancellationToken cancellationToken)
    {
        Result<List<KeycloakRoleRepresentation>> result = await this.SendForResponse<List<KeycloakRoleRepresentation>>(HttpMethod.Get,
            $"/admin/realms/{this.GetRealm(null)}/roles", null, cancellationToken);

        if (result.IsFailed)
        {
            return ResultHelpers.CreateFailure(result);
        }

        return Result.Success(result.Data.Select(MapRole).ToList());
    }

    public async Task<Result> CreateUser(SecurityServiceCommands.CreateUserCommand command, CancellationToken cancellationToken)
    {
        KeycloakUserProviderSettings providerSettings = GetProviderSettings<KeycloakUserProviderSettings>(command.ProviderSettings);
        String realm = this.GetRealm(providerSettings.Realm);
        if (String.IsNullOrWhiteSpace(realm))
        {
            return Result.Invalid("No Keycloak realm is configured for user management");
        }

        KeycloakUserRepresentation request = new KeycloakUserRepresentation
                                             {
                                                 Attributes = MapAttributes(command),
                                                 Email = command.EmailAddress,
                                                 EmailVerified = providerSettings.EmailVerified ?? false,
                                                 Enabled = providerSettings.Enabled ?? true,
                                                 FirstName = command.GivenName,
                                                 Id = command.UserId.ToString(),
                                                 LastName = command.FamilyName,
                                                 RequiredActions = providerSettings.RequiredActions ?? new List<String>(),
                                                 Username = command.UserName
                                             };

        if (String.IsNullOrWhiteSpace(command.Password) == false)
        {
            request.Credentials.Add(new KeycloakCredentialRepresentation
                                    {
                                        Temporary = false,
                                        Type = "password",
                                        Value = command.Password
                                    });
        }

        Result createResult = await this.SendWithoutResponse(HttpMethod.Post, $"/admin/realms/{realm}/users", request, cancellationToken);
        if (createResult.IsFailed)
        {
            return createResult;
        }

        String userId = await this.FindUserId(realm, command.UserName, cancellationToken);
        if (String.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure($"User [{command.UserName}] was created in Keycloak but could not be reloaded for follow-up configuration");
        }

        Result rolesResult = await this.AssignRealmRoles(realm, userId, command.Roles, cancellationToken);
        if (rolesResult.IsFailed)
        {
            return rolesResult;
        }

        return await this.AssignGroups(realm, userId, providerSettings.Groups, cancellationToken);
    }

    public async Task<Result<UserDetails>> GetUser(SecurityServiceQueries.GetUserQuery query, CancellationToken cancellationToken)
    {
        Result<KeycloakUserRepresentation> userResult = await this.SendForResponse<KeycloakUserRepresentation>(HttpMethod.Get,
            $"/admin/realms/{this.GetRealm(null)}/users/{query.UserId}", null, cancellationToken);

        if (userResult.IsFailed)
        {
            return ResultHelpers.CreateFailure(userResult);
        }

        List<String> roles = await this.GetUserRoles(this.GetRealm(null), query.UserId.ToString(), cancellationToken);
        return Result.Success(MapUser(query.UserId, userResult.Data, roles));
    }

    public async Task<Result<List<UserDetails>>> GetUsers(SecurityServiceQueries.GetUsersQuery query, CancellationToken cancellationToken)
    {
        String requestUri = String.IsNullOrWhiteSpace(query.UserName)
            ? $"/admin/realms/{this.GetRealm(null)}/users"
            : $"/admin/realms/{this.GetRealm(null)}/users?username={Uri.EscapeDataString(query.UserName)}";

        Result<List<KeycloakUserRepresentation>> result = await this.SendForResponse<List<KeycloakUserRepresentation>>(HttpMethod.Get, requestUri, null, cancellationToken);
        if (result.IsFailed)
        {
            return ResultHelpers.CreateFailure(result);
        }

        List<UserDetails> users = new List<UserDetails>();
        foreach (KeycloakUserRepresentation user in result.Data)
        {
            Guid.TryParse(user.Id, out Guid userId);
            List<String> roles = await this.GetUserRoles(this.GetRealm(null), user.Id, cancellationToken);
            users.Add(MapUser(userId, user, roles));
        }

        return Result.Success(users);
    }

    public async Task<Result<ChangeUserPasswordResult>> ChangeUserPassword(SecurityServiceCommands.ChangeUserPasswordCommand command, CancellationToken cancellationToken)
    {
        String realm = this.GetRealm(null);
        String userId = await this.FindUserId(realm, command.UserName, cancellationToken);
        if (String.IsNullOrWhiteSpace(userId))
        {
            return Result.NotFound($"No user found with username {command.UserName}");
        }

        KeycloakCredentialRepresentation request = new KeycloakCredentialRepresentation
                                                  {
                                                      Temporary = false,
                                                      Type = "password",
                                                      Value = command.NewPassword
                                                  };

        Result resetPasswordResult = await this.SendWithoutResponse(HttpMethod.Put, $"/admin/realms/{realm}/users/{userId}/reset-password", request, cancellationToken, HttpStatusCode.NoContent);
        if (resetPasswordResult.IsFailed)
        {
            return ResultHelpers.CreateFailure<ChangeUserPasswordResult>(resetPasswordResult);
        }

        Result<Client> clientResult = await this.GetClient(new SecurityServiceQueries.GetClientQuery(command.ClientId), cancellationToken);
        if (clientResult.IsFailed)
        {
            return ResultHelpers.CreateFailure<ChangeUserPasswordResult>(clientResult);
        }

        return Result.Success(new ChangeUserPasswordResult
                              {
                                  IsSuccessful = true,
                                  RedirectUri = clientResult.Data.ClientUri
                              });
    }

    public Task<Result> ConfirmUserEmailAddress(SecurityServiceCommands.ConfirmUserEmailAddressCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported("email confirmation"));

    public Task<Result<string>> ProcessPasswordResetConfirmation(SecurityServiceCommands.ProcessPasswordResetConfirmationCommand command, CancellationToken cancellationToken) =>
        Task.FromResult(Unsupported<String>("password reset confirmation"));

    public async Task<Result> ProcessPasswordResetRequest(SecurityServiceCommands.ProcessPasswordResetRequestCommand command, CancellationToken cancellationToken)
    {
        String realm = this.GetRealm(null);
        String userId = await this.FindUserId(realm, command.Username, cancellationToken);
        if (String.IsNullOrWhiteSpace(userId))
        {
            return Result.NotFound($"No user found with username {command.Username}");
        }

        return await this.SendExecuteActionsEmail(realm, userId, new List<String> { "UPDATE_PASSWORD" }, command.ClientId, cancellationToken);
    }

    public async Task<Result> SendWelcomeEmail(SecurityServiceCommands.SendWelcomeEmailCommand command, CancellationToken cancellationToken)
    {
        String realm = this.GetRealm(null);
        String userId = await this.FindUserId(realm, command.Username, cancellationToken);
        if (String.IsNullOrWhiteSpace(userId))
        {
            return Result.NotFound($"No user found with username {command.Username}");
        }

        return await this.SendExecuteActionsEmail(realm, userId, new List<String> { "VERIFY_EMAIL", "UPDATE_PASSWORD" }, null, cancellationToken);
    }

    private async Task<Result> AssignGroups(String realm, String userId, List<String> groups, CancellationToken cancellationToken)
    {
        if (groups == null || groups.Any() == false)
        {
            return Result.Success();
        }

        foreach (String groupName in groups)
        {
            Result<List<KeycloakGroupRepresentation>> groupResult = await this.SendForResponse<List<KeycloakGroupRepresentation>>(HttpMethod.Get,
                $"/admin/realms/{realm}/groups?search={Uri.EscapeDataString(groupName)}", null, cancellationToken);

            if (groupResult.IsFailed)
            {
                return ResultHelpers.CreateFailure(groupResult);
            }

            KeycloakGroupRepresentation group = groupResult.Data.FirstOrDefault(g => String.Equals(g.Name, groupName, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                return Result.Invalid($"Keycloak group [{groupName}] was not found");
            }

            Result addGroupResult = await this.SendWithoutResponse(HttpMethod.Put, $"/admin/realms/{realm}/users/{userId}/groups/{group.Id}", null, cancellationToken, HttpStatusCode.NoContent);
            if (addGroupResult.IsFailed)
            {
                return addGroupResult;
            }
        }

        return Result.Success();
    }

    private async Task<Result> AssignRealmRoles(String realm, String userId, List<String> roles, CancellationToken cancellationToken)
    {
        if (roles == null || roles.Any() == false)
        {
            return Result.Success();
        }

        List<KeycloakRoleRepresentation> realmRoles = new List<KeycloakRoleRepresentation>();
        foreach (String roleName in roles)
        {
            Result<KeycloakRoleRepresentation> roleResult = await this.SendForResponse<KeycloakRoleRepresentation>(HttpMethod.Get,
                $"/admin/realms/{realm}/roles/{Uri.EscapeDataString(roleName)}", null, cancellationToken);

            if (roleResult.IsFailed)
            {
                return ResultHelpers.CreateFailure(roleResult);
            }

            realmRoles.Add(roleResult.Data);
        }

        return await this.SendWithoutResponse(HttpMethod.Post, $"/admin/realms/{realm}/users/{userId}/role-mappings/realm", realmRoles, cancellationToken, HttpStatusCode.NoContent);
    }

    private static Dictionary<String, List<String>> MapAttributes(SecurityServiceCommands.CreateUserCommand command)
    {
        Dictionary<String, List<String>> attributes = new Dictionary<String, List<String>>(StringComparer.OrdinalIgnoreCase);

        if (command.Claims != null)
        {
            foreach (KeyValuePair<String, String> claim in command.Claims)
            {
                attributes[claim.Key] = new List<String> { claim.Value };
            }
        }

        if (String.IsNullOrWhiteSpace(command.MiddleName) == false)
        {
            attributes["middle_name"] = new List<String> { command.MiddleName };
        }

        if (String.IsNullOrWhiteSpace(command.PhoneNumber) == false)
        {
            attributes["phone_number"] = new List<String> { command.PhoneNumber };
        }

        return attributes;
    }

    private async Task<String> FindUserId(String realm, String username, CancellationToken cancellationToken)
    {
        Result<List<KeycloakUserRepresentation>> result = await this.SendForResponse<List<KeycloakUserRepresentation>>(HttpMethod.Get,
            $"/admin/realms/{realm}/users?username={Uri.EscapeDataString(username)}", null, cancellationToken);

        if (result.IsFailed)
        {
            return null;
        }

        return result.Data.FirstOrDefault(u => String.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase))?.Id;
    }

    private String GetRealm(String realmOverride) =>
        String.IsNullOrWhiteSpace(realmOverride) ? this.ServiceOptions.Keycloak?.Realm : realmOverride;

    private async Task<List<KeycloakClientRepresentation>> GetClientsInternal(String realm, String clientId, CancellationToken cancellationToken)
    {
        String requestUri = String.IsNullOrWhiteSpace(clientId)
            ? $"/admin/realms/{realm}/clients"
            : $"/admin/realms/{realm}/clients?clientId={Uri.EscapeDataString(clientId)}";

        Result<List<KeycloakClientRepresentation>> result = await this.SendForResponse<List<KeycloakClientRepresentation>>(HttpMethod.Get, requestUri, null, cancellationToken);
        return result.IsSuccess ? result.Data : new List<KeycloakClientRepresentation>();
    }

    private async Task<List<String>> GetUserRoles(String realm, String userId, CancellationToken cancellationToken)
    {
        Result<List<KeycloakRoleRepresentation>> rolesResult = await this.SendForResponse<List<KeycloakRoleRepresentation>>(HttpMethod.Get,
            $"/admin/realms/{realm}/users/{userId}/role-mappings/realm", null, cancellationToken);

        return rolesResult.IsSuccess ? rolesResult.Data.Select(r => r.Name).Where(n => String.IsNullOrWhiteSpace(n) == false).ToList() : new List<String>();
    }

    private async Task<Result> SendExecuteActionsEmail(String realm, String userId, List<String> actions, String clientId, CancellationToken cancellationToken)
    {
        Dictionary<String, Object> request = new Dictionary<String, Object>
                                             {
                                                 { "actions", actions }
                                             };

        if (String.IsNullOrWhiteSpace(clientId) == false)
        {
            request["clientId"] = clientId;
        }

        return await this.SendWithoutResponse(HttpMethod.Put, $"/admin/realms/{realm}/users/{userId}/execute-actions-email", request, cancellationToken, HttpStatusCode.NoContent);
    }

    private async Task<Result<T>> SendForResponse<T>(HttpMethod method, String relativeUri, Object body, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = await this.CreateRequest(method, relativeUri, body, cancellationToken);
        using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode == false)
        {
            String failureMessage = await BuildFailureMessage(response);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result.NotFound(failureMessage);
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return Result.Conflict(failureMessage);
            }

            return Result.Failure(failureMessage);
        }

        if (response.Content == null)
        {
            return Result.Success(default(T));
        }

        String content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (String.IsNullOrWhiteSpace(content))
        {
            return Result.Success(default(T));
        }

        T responseBody = JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
        return Result.Success(responseBody);
    }

    private async Task<Result> SendWithoutResponse(HttpMethod method,
                                                   String relativeUri,
                                                   Object body,
                                                   CancellationToken cancellationToken,
                                                   params HttpStatusCode[] validStatusCodes)
    {
        HttpStatusCode[] expectedStatusCodes = validStatusCodes != null && validStatusCodes.Any()
            ? validStatusCodes
            : new[] { HttpStatusCode.Created, HttpStatusCode.NoContent, HttpStatusCode.OK };

        HttpRequestMessage request = await this.CreateRequest(method, relativeUri, body, cancellationToken);
        using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);

        if (expectedStatusCodes.Contains(response.StatusCode))
        {
            return Result.Success();
        }

        String failureMessage = await BuildFailureMessage(response);
        return response.StatusCode switch
               {
                   HttpStatusCode.Conflict => Result.Conflict(failureMessage),
                   HttpStatusCode.NotFound => Result.NotFound(failureMessage),
                   HttpStatusCode.BadRequest => Result.Invalid(failureMessage),
                   _ => Result.Failure(failureMessage)
               };
    }

    private async Task<HttpRequestMessage> CreateRequest(HttpMethod method, String relativeUri, Object body, CancellationToken cancellationToken)
    {
        String accessToken = await this.GetAccessToken(cancellationToken);
        HttpRequestMessage request = new HttpRequestMessage(method, $"{this.ServiceOptions.Keycloak.ServerUrl.TrimEnd('/')}{relativeUri}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (body != null)
        {
            String json = JsonSerializer.Serialize(body, JsonSerializerOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private async Task<String> GetAccessToken(CancellationToken cancellationToken)
    {
        if (this.TokenResponse != null && this.TokenResponse.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return this.TokenResponse.AccessToken;
        }

        String serverUrl = this.ServiceOptions.Keycloak?.ServerUrl?.TrimEnd('/');
        if (String.IsNullOrWhiteSpace(serverUrl))
        {
            throw new InvalidOperationException("Keycloak server URL has not been configured");
        }

        Dictionary<String, String> formValues = new Dictionary<String, String>
                                                {
                                                    { "grant_type", "client_credentials" },
                                                    { "client_id", this.ServiceOptions.Keycloak.AdminClientId },
                                                    { "client_secret", this.ServiceOptions.Keycloak.AdminClientSecret }
                                                };

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
            $"{serverUrl}/realms/{this.ServiceOptions.Keycloak.AdminRealm}/protocol/openid-connect/token")
                                          {
                                              Content = new FormUrlEncodedContent(formValues)
                                          };
        using HttpResponseMessage response = await this.HttpClient.SendAsync(request, cancellationToken);
        String content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode == false)
        {
            throw new InvalidOperationException($"Failed to acquire Keycloak admin token. Response was [{response.StatusCode}] {content}");
        }

        KeycloakTokenEndpointResponse tokenEndpointResponse = JsonSerializer.Deserialize<KeycloakTokenEndpointResponse>(content, JsonSerializerOptions);
        this.TokenResponse = new KeycloakTokenResponse
                             {
                                 AccessToken = tokenEndpointResponse.AccessToken,
                                 ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenEndpointResponse.ExpiresIn)
                             };
        Logger.LogInformation($"Acquired Keycloak admin token that expires at {this.TokenResponse.ExpiresAt:O}");
        return this.TokenResponse.AccessToken;
    }

    private static Client MapClient(KeycloakClientRepresentation client)
    {
        List<String> grantTypes = new List<String>();
        if (client.StandardFlowEnabled)
        {
            grantTypes.Add(GrantType.AuthorizationCode);
        }

        if (client.ImplicitFlowEnabled)
        {
            grantTypes.Add(GrantType.Implicit);
        }

        if (client.DirectAccessGrantsEnabled)
        {
            grantTypes.Add(GrantType.ResourceOwnerPassword);
        }

        if (client.ServiceAccountsEnabled)
        {
            grantTypes.Add(GrantType.ClientCredentials);
        }

        List<String> postLogoutRedirectUris = new List<String>();
        if (client.Attributes.TryGetValue("post.logout.redirect.uris", out String configuredPostLogoutUris) &&
            String.IsNullOrWhiteSpace(configuredPostLogoutUris) == false)
        {
            postLogoutRedirectUris = configuredPostLogoutUris.Split("##", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        return new Client
               {
                   AllowedGrantTypes = grantTypes,
                   AllowedScopes = new List<String>(),
                   AllowOfflineAccess = false,
                   ClientId = client.ClientId,
                   ClientName = client.Name,
                   ClientUri = client.RootUrl ?? client.BaseUrl ?? client.AdminUrl,
                   Description = client.Description,
                   PostLogoutRedirectUris = postLogoutRedirectUris,
                   RedirectUris = client.RedirectUris ?? new List<String>(),
                   RequireConsent = false
               };
    }

    private static RoleDetails MapRole(KeycloakRoleRepresentation role)
    {
        Guid.TryParse(role.Id, out Guid roleId);
        return new RoleDetails
               {
                   RoleId = roleId,
                   RoleName = role.Name
               };
    }

    private static UserDetails MapUser(Guid userId, KeycloakUserRepresentation user, List<String> roles)
    {
        Dictionary<String, String> claims = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase);
        if (user.Attributes != null)
        {
            foreach (KeyValuePair<String, List<String>> attribute in user.Attributes)
            {
                String value = attribute.Value?.FirstOrDefault();
                if (String.IsNullOrWhiteSpace(value) == false)
                {
                    claims[attribute.Key] = value;
                }
            }
        }

        if (String.IsNullOrWhiteSpace(user.Email) == false)
        {
            claims[JwtClaimTypes.Email] = user.Email;
        }

        if (String.IsNullOrWhiteSpace(user.FirstName) == false)
        {
            claims[JwtClaimTypes.GivenName] = user.FirstName;
        }

        if (String.IsNullOrWhiteSpace(user.LastName) == false)
        {
            claims[JwtClaimTypes.FamilyName] = user.LastName;
        }

        return new UserDetails
               {
                   Claims = claims,
                   Email = user.Email,
                   PhoneNumber = user.Attributes != null && user.Attributes.TryGetValue("phone_number", out List<String> phoneNumbers)
                       ? phoneNumbers.FirstOrDefault()
                       : null,
                   Roles = roles,
                   SubjectId = user.Id,
                   UserId = userId,
                   Username = user.Username
               };
    }

    private static T GetProviderSettings<T>(Dictionary<String, Object> providerSettings) where T : new()
    {
        if (providerSettings == null || providerSettings.Any() == false)
        {
            return new T();
        }

        Dictionary<String, JsonElement> normalized = JsonSerializer.Deserialize<Dictionary<String, JsonElement>>(
            JsonSerializer.Serialize(providerSettings, JsonSerializerOptions), JsonSerializerOptions);

        if (normalized == null || normalized.TryGetValue("keycloak", out JsonElement keycloakSettings) == false)
        {
            return new T();
        }

        return JsonSerializer.Deserialize<T>(keycloakSettings.GetRawText(), JsonSerializerOptions) ?? new T();
    }

    private static async Task<String> BuildFailureMessage(HttpResponseMessage response)
    {
        String responseBody = response.Content == null ? String.Empty : await response.Content.ReadAsStringAsync();
        return $"Keycloak request failed with status code [{(Int32)response.StatusCode}] {response.StatusCode}. Response: {responseBody}";
    }

    private static Result Unsupported(String entityName) =>
        Result.Invalid($"Keycloak identity management service does not support {entityName} management through this endpoint");

    private static Result<T> Unsupported<T>(String entityName) =>
        Result.Invalid($"Keycloak identity management service does not support {entityName} management through this endpoint");

    private static Result ValidateGrantTypes(List<String> allowedGrantTypes)
    {
        List<String> validTypesList = new List<String>
                                      {
                                          GrantType.AuthorizationCode,
                                          GrantType.ClientCredentials,
                                          GrantType.Hybrid,
                                          GrantType.Implicit,
                                          GrantType.ResourceOwnerPassword
                                      };

        List<String> invalidGrantTypes = allowedGrantTypes == null
            ? new List<String>()
            : allowedGrantTypes.Where(a => validTypesList.All(v => v != a)).ToList();

        return invalidGrantTypes.Any()
            ? Result.Invalid($"The grant types [{String.Join(", ", invalidGrantTypes)}] are not valid to create a new Keycloak client")
            : Result.Success();
    }

    private sealed class KeycloakTokenResponse
    {
        public String AccessToken { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }
    }

    private sealed class KeycloakTokenEndpointResponse
    {
        [JsonPropertyName("access_token")]
        public String AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public Int32 ExpiresIn { get; set; }
    }

    private sealed class KeycloakClientRepresentation
    {
        [JsonPropertyName("adminUrl")]
        public String AdminUrl { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<String, String> Attributes { get; set; } = new Dictionary<String, String>();

        [JsonPropertyName("baseUrl")]
        public String BaseUrl { get; set; }

        [JsonPropertyName("clientId")]
        public String ClientId { get; set; }

        [JsonPropertyName("description")]
        public String Description { get; set; }

        [JsonPropertyName("directAccessGrantsEnabled")]
        public Boolean DirectAccessGrantsEnabled { get; set; }

        [JsonPropertyName("frontchannelLogout")]
        public Boolean FrontchannelLogout { get; set; }

        [JsonPropertyName("implicitFlowEnabled")]
        public Boolean ImplicitFlowEnabled { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("protocol")]
        public String Protocol { get; set; } = "openid-connect";

        [JsonPropertyName("publicClient")]
        public Boolean PublicClient { get; set; }

        [JsonPropertyName("redirectUris")]
        public List<String> RedirectUris { get; set; } = new List<String>();

        [JsonPropertyName("rootUrl")]
        public String RootUrl { get; set; }

        [JsonPropertyName("secret")]
        public String Secret { get; set; }

        [JsonPropertyName("serviceAccountsEnabled")]
        public Boolean ServiceAccountsEnabled { get; set; }

        [JsonPropertyName("standardFlowEnabled")]
        public Boolean StandardFlowEnabled { get; set; }

        [JsonPropertyName("webOrigins")]
        public List<String> WebOrigins { get; set; } = new List<String>();
    }

    private sealed class KeycloakRoleRepresentation
    {
        [JsonPropertyName("id")]
        public String Id { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }
    }

    private sealed class KeycloakUserRepresentation
    {
        [JsonPropertyName("attributes")]
        public Dictionary<String, List<String>> Attributes { get; set; } = new Dictionary<String, List<String>>();

        [JsonPropertyName("credentials")]
        public List<KeycloakCredentialRepresentation> Credentials { get; set; } = new List<KeycloakCredentialRepresentation>();

        [JsonPropertyName("email")]
        public String Email { get; set; }

        [JsonPropertyName("emailVerified")]
        public Boolean EmailVerified { get; set; }

        [JsonPropertyName("enabled")]
        public Boolean Enabled { get; set; }

        [JsonPropertyName("firstName")]
        public String FirstName { get; set; }

        [JsonPropertyName("id")]
        public String Id { get; set; }

        [JsonPropertyName("lastName")]
        public String LastName { get; set; }

        [JsonPropertyName("requiredActions")]
        public List<String> RequiredActions { get; set; } = new List<String>();

        [JsonPropertyName("username")]
        public String Username { get; set; }
    }

    private sealed class KeycloakCredentialRepresentation
    {
        [JsonPropertyName("temporary")]
        public Boolean Temporary { get; set; }

        [JsonPropertyName("type")]
        public String Type { get; set; }

        [JsonPropertyName("value")]
        public String Value { get; set; }
    }

    private sealed class KeycloakGroupRepresentation
    {
        [JsonPropertyName("id")]
        public String Id { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }
    }

    private sealed class KeycloakClientProviderSettings
    {
        [JsonPropertyName("client_type")]
        public String ClientType { get; set; }

        [JsonPropertyName("direct_access_grants_enabled")]
        public Boolean? DirectAccessGrantsEnabled { get; set; }

        [JsonPropertyName("frontchannel_logout")]
        public Boolean? FrontchannelLogout { get; set; }

        [JsonPropertyName("implicit_flow_enabled")]
        public Boolean? ImplicitFlowEnabled { get; set; }

        [JsonPropertyName("public_client")]
        public Boolean? PublicClient { get; set; }

        [JsonPropertyName("realm")]
        public String Realm { get; set; }

        [JsonPropertyName("service_accounts_enabled")]
        public Boolean? ServiceAccountsEnabled { get; set; }

        [JsonPropertyName("standard_flow_enabled")]
        public Boolean? StandardFlowEnabled { get; set; }

        [JsonPropertyName("web_origins")]
        public List<String> WebOrigins { get; set; }
    }

    private sealed class KeycloakUserProviderSettings
    {
        [JsonPropertyName("email_verified")]
        public Boolean? EmailVerified { get; set; }

        [JsonPropertyName("enabled")]
        public Boolean? Enabled { get; set; }

        [JsonPropertyName("groups")]
        public List<String> Groups { get; set; }

        [JsonPropertyName("realm")]
        public String Realm { get; set; }

        [JsonPropertyName("required_actions")]
        public List<String> RequiredActions { get; set; }
    }
}
