namespace SecurityService.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using Duende.IdentityServer.Models;

    public static class ModelFactory {
        #region Methods

        public static UserDetails ConvertFrom(Models.UserDetails model) {
            if (model == null) {
                return null;
            }

            return new UserDetails {
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                Roles = model.Roles,
                Claims = model.Claims,
                UserId = model.UserId,
                EmailAddress = model.Email,
                RegistrationDateTime = model.RegistrationDateTime
            };
        }

        public static RoleDetails ConvertFrom(Models.RoleDetails model) {
            if (model == null) {
                return null;
            }

            return new RoleDetails { RoleId = model.RoleId, RoleName = model.RoleName };
        }

        public static List<UserDetails> ConvertFrom(List<Models.UserDetails> model) {
            if (model == null || model.Any() == false) {
                return new List<UserDetails>();
            }

            List<UserDetails> userDetailsList = new List<UserDetails>();

            foreach (Models.UserDetails userDetails in model) {
                userDetailsList.Add(ConvertFrom(userDetails));
            }

            return userDetailsList;
        }

        public static List<RoleDetails> ConvertFrom(List<Models.RoleDetails> model) {
            if (model == null || model.Any() == false) {
                return new List<RoleDetails>();
            }

            List<RoleDetails> roleDetailsList = new List<RoleDetails>();

            foreach (Models.RoleDetails roleDetails in model) {
                roleDetailsList.Add(ConvertFrom(roleDetails));
            }

            return roleDetailsList;
        }

        public static ClientDetails ConvertFrom(Client model) {
            if (model == null) {
                return null;
            }

            return new ClientDetails {
                ClientId = model.ClientId,
                AllowedScopes = model.AllowedScopes.ToList(),
                AllowedGrantTypes = model.AllowedGrantTypes.ToList(),
                ClientName = model.ClientName,
                ClientDescription = model.Description,
                Enabled = model.Enabled,
                ClientPostLogoutRedirectUris = model.PostLogoutRedirectUris.ToList(),
                RequireConsent = model.RequireConsent,
                ClientRedirectUris = model.RedirectUris.ToList(),
                AllowOfflineAccess = model.AllowOfflineAccess
            };
        }

        public static List<ClientDetails> ConvertFrom(List<Client> model) {
            if (model == null || model.Any() == false) {
                return new List<ClientDetails>();
            }

            List<ClientDetails> clientDetailsList = new List<ClientDetails>();

            foreach (Client client in model) {
                clientDetailsList.Add(ConvertFrom(client));
            }

            return clientDetailsList;
        }

        public static ApiResourceDetails ConvertFrom(ApiResource model) {
            if (model == null) {
                return null;
            }

            return new ApiResourceDetails {
                Description = model.Description,
                Scopes = model.Scopes.ToList(),
                Name = model.Name,
                DisplayName = model.DisplayName,
                UserClaims = model.UserClaims.ToList(),
                Enabled = model.Enabled
            };
        }

        public static ApiScopeDetails ConvertFrom(ApiScope model) {
            if (model == null) {
                return null;
            }

            return new ApiScopeDetails { Enabled = model.Enabled, DisplayName = model.DisplayName, Name = model.Name, Description = model.Description };
        }

        public static List<ApiScopeDetails> ConvertFrom(List<ApiScope> model) {
            if (model == null || model.Any() == false) {
                return new List<ApiScopeDetails>();
            }

            List<ApiScopeDetails> apiScopeDetailsList = new List<ApiScopeDetails>();

            foreach (ApiScope apiScope in model) {
                apiScopeDetailsList.Add(ConvertFrom(apiScope));
            }

            return apiScopeDetailsList;
        }

        public static IdentityResourceDetails ConvertFrom(IdentityResource model) {
            if (model == null) {
                return null;
            }

            return new IdentityResourceDetails {
                Claims = model.UserClaims.ToList(),
                DisplayName = model.DisplayName,
                Emphasize = model.Emphasize,
                Required = model.Required,
                ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                Description = model.Description,
                Name = model.Name
            };
        }

        public static List<ApiResourceDetails> ConvertFrom(List<ApiResource> model) {
            if (model == null || model.Any() == false) {
                return new List<ApiResourceDetails>();
            }

            List<ApiResourceDetails> apiResourceDetailsList = new List<ApiResourceDetails>();

            foreach (ApiResource apiResource in model) {
                apiResourceDetailsList.Add(ConvertFrom(apiResource));
            }

            return apiResourceDetailsList;
        }

        public static List<IdentityResourceDetails> ConvertFrom(List<IdentityResource> model) {
            if (model == null || model.Any() == false) {
                return new List<IdentityResourceDetails>();
            }

            List<IdentityResourceDetails> identityResourceDetailsList = new List<IdentityResourceDetails>();

            foreach (IdentityResource identityResource in model) {
                identityResourceDetailsList.Add(ConvertFrom(identityResource));
            }

            return identityResourceDetailsList;
        }

        #endregion
    }
}