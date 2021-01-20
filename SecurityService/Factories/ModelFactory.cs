namespace SecurityService.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using IdentityServer4.Models;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="SecurityService.Factories.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public UserDetails ConvertFrom(Models.UserDetails model)
        {
            if (model == null)
            {
                return null;
            }

            return new UserDetails
                   {
                       UserName = model.Username,
                       PhoneNumber = model.PhoneNumber,
                       Roles = model.Roles,
                       Claims = model.Claims,
                       UserId = model.UserId,
                       EmailAddress = model.Email
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public RoleDetails ConvertFrom(Models.RoleDetails model)
        {
            if (model == null)
            {
                return null;
            }

            return new RoleDetails
                   {
                       RoleId = model.RoleId,
                       RoleName = model.RoleName
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<UserDetails> ConvertFrom(List<Models.UserDetails> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<UserDetails> userDetailsList = new List<UserDetails>();

            foreach (Models.UserDetails userDetails in model)
            {
                userDetailsList.Add(this.ConvertFrom(userDetails));
            }

            return userDetailsList;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<RoleDetails> ConvertFrom(List<Models.RoleDetails> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<RoleDetails> roleDetailsList = new List<RoleDetails>();

            foreach (Models.RoleDetails roleDetails in model)
            {
                roleDetailsList.Add(this.ConvertFrom(roleDetails));
            }

            return roleDetailsList;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public ClientDetails ConvertFrom(Client model)
        {
            if (model == null)
            {
                return null;
            }

            return new ClientDetails
                   {
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

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<ClientDetails> ConvertFrom(List<Client> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<ClientDetails> clientDetailsList = new List<ClientDetails>();

            foreach (Client client in model)
            {
                clientDetailsList.Add(this.ConvertFrom(client));
            }

            return clientDetailsList;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public ApiResourceDetails ConvertFrom(ApiResource model)
        {
            if (model == null)
            {
                return null;
            }

            return new ApiResourceDetails
                   {
                       Description = model.Description,
                       Scopes = model.Scopes.ToList(),
                       Name = model.Name,
                       DisplayName = model.DisplayName,
                       UserClaims = model.UserClaims.ToList(),
                       Enabled = model.Enabled
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public ApiScopeDetails ConvertFrom(ApiScope model)
        {
            if (model == null)
            {
                return null;
            }

            return new ApiScopeDetails
                   {
                       Enabled = model.Enabled,
                       DisplayName = model.DisplayName,
                       Name = model.Name,
                       Description = model.Description
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<ApiScopeDetails> ConvertFrom(List<ApiScope> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<ApiScopeDetails> apiScopeDetailsList = new List<ApiScopeDetails>();

            foreach (ApiScope apiScope in model)
            {
                apiScopeDetailsList.Add(this.ConvertFrom(apiScope));
            }

            return apiScopeDetailsList;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public IdentityResourceDetails ConvertFrom(IdentityResource model)
        {
            if (model == null)
            {
                return null;
            }

            return new IdentityResourceDetails
                   {
                       Claims = model.UserClaims.ToList(),
                       DisplayName = model.DisplayName,
                       Emphasize = model.Emphasize,
                       Required = model.Required,
                       ShowInDiscoveryDocument = model.ShowInDiscoveryDocument,
                       Description = model.Description,
                       Name = model.Name
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<ApiResourceDetails> ConvertFrom(List<ApiResource> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<ApiResourceDetails> apiResourceDetailsList = new List<ApiResourceDetails>();

            foreach (ApiResource apiResource in model)
            {
                apiResourceDetailsList.Add(this.ConvertFrom(apiResource));
            }

            return apiResourceDetailsList;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<IdentityResourceDetails> ConvertFrom(List<IdentityResource> model)
        {
            if (model == null || model.Any() == false)
            {
                return null;
            }

            List<IdentityResourceDetails> identityResourceDetailsList = new List<IdentityResourceDetails>();

            foreach (IdentityResource identityResource in model)
            {
                identityResourceDetailsList.Add(this.ConvertFrom(identityResource));
            }

            return identityResourceDetailsList;
        }

        #endregion
    }
}