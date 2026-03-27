using SecurityService.DataTransferObjects;
using SecurityService.Models;

namespace SecurityService.Factories
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ModelFactory {
        #region Methods

       public static UserResponse ConvertFrom(UserDetails model) {
            if (model == null) {
                return null;
            }

            return new UserResponse
            {
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Roles = model.Roles,
                Claims = model.Claims,
                UserId = model.UserId,
                EmailAddress = model.EmailAddress,
                //RegistrationDateTime = model.RegistrationDateTime
                FamilyName = model.FamilyName,
                GivenName = model.GivenName,
                MiddleName = model.MiddleName
            };
        }

        public static RoleResponse ConvertFrom(RoleDetails model) {
            if (model == null) {
                return null;
            }

            return new RoleResponse { RoleId = model.RoleId, Name = model.Name };
        }

        public static List<UserResponse> ConvertFrom(List<UserDetails> model) {
            if (model == null || model.Any() == false) {
                return new List<UserResponse>();
            }

            List<UserResponse> userDetailsList = new List<UserResponse>();

            foreach (UserDetails userDetails in model) {
                userDetailsList.Add(ConvertFrom(userDetails));
            }

            return userDetailsList;
        }

        public static List<RoleResponse> ConvertFrom(List<RoleDetails> model) {
            if (model == null || model.Any() == false) {
                return new List<RoleResponse>();
            }

            List<RoleResponse> roleDetailsList = new List<RoleResponse>();

            foreach (RoleDetails roleDetails in model) {
                roleDetailsList.Add(ConvertFrom(roleDetails));
            }

            return roleDetailsList;
        }


       public static ClientResponse ConvertFrom(ClientDetails model) {
            if (model == null) {
                return null;
            }

            return new ClientResponse
            {
                ClientId = model.ClientId,
                AllowedScopes = model.AllowedScopes.ToList(),
                AllowedGrantTypes = model.AllowedGrantTypes.ToList(),
                ClientName = model.ClientName,
                Description = model.Description,
                PostLogoutRedirectUris = model.PostLogoutRedirectUris.ToList(),
                RequireConsent = model.RequireConsent,
                RedirectUris = model.RedirectUris.ToList(),
                AllowOfflineAccess = model.AllowOfflineAccess,
                ClientType = model.ClientType,
                ClientUri = model.ClientUri
            };
        }

        public static List<ClientResponse> ConvertFrom(List<ClientDetails> model) {
            if (model == null || model.Any() == false) {
                return new List<ClientResponse>();
            }

            List<ClientResponse> clientDetailsList = new List<ClientResponse>();

            foreach (ClientDetails client in model) {
                clientDetailsList.Add(ConvertFrom(client));
            }

            return clientDetailsList;
        }

        public static ApiResourceResponse ConvertFrom(ApiResourceDetails model) {
            if (model == null) {
                return null;
            }

            return new ApiResourceResponse
            {
                Description = model.Description,
                Scopes = model.Scopes.ToList(),
                Name = model.Name,
                DisplayName = model.DisplayName,
                UserClaims = model.UserClaims.ToList(),
            };
        }

        public static List<ApiResourceResponse> ConvertFrom(List<ApiResourceDetails> model)
        {
            if (model == null || model.Any() == false)
            {
                return new List<ApiResourceResponse>();
            }

            List<ApiResourceResponse> apiResourceResponseList = new List<ApiResourceResponse>();

            foreach (ApiResourceDetails apiResource in model)
            {
                apiResourceResponseList.Add(ConvertFrom(apiResource));
            }

            return apiResourceResponseList;
        }

        public static ApiScopeResponse ConvertFrom(ApiScopeDetails model)
        {
            if (model == null)
            {
                return null;
            }

            return new ApiScopeResponse { DisplayName = model.DisplayName, Name = model.Name, Description = model.Description };
        }

        public static List<ApiScopeResponse> ConvertFrom(List<ApiScopeDetails> model)
        {
            if (model == null || model.Any() == false)
            {
                return new List<ApiScopeResponse>();
            }

            List<ApiScopeResponse> apiScopeDetailsList = new List<ApiScopeResponse>();

            foreach (ApiScopeDetails apiScope in model)
            {
                apiScopeDetailsList.Add(ConvertFrom(apiScope));
            }

            return apiScopeDetailsList;
        }

        public static IdentityResourceResponse ConvertFrom(IdentityResourceDetails model)
        {
            if (model == null)
            {
                return null;
            }

            return new IdentityResourceResponse
            {
                Claims = model.Claims.ToList(),
                DisplayName = model.DisplayName,
                Emphasize = model.Emphasize,
                Required = model.Required,
                ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                Description = model.Description,
                Name = model.Name
            };
        }



        public static List<IdentityResourceResponse> ConvertFrom(List<SecurityService.Models.IdentityResourceDetails> model)
        {
            if (model == null || model.Any() == false)
            {
                return new List<IdentityResourceResponse>();
            }

            List<IdentityResourceResponse> identityResourceDetailsList = new List<IdentityResourceResponse>();

            foreach (IdentityResourceDetails identityResource in model)
            {
                identityResourceDetailsList.Add(ConvertFrom(identityResource));
            }

            return identityResourceDetailsList;
        }

        #endregion
    }
}