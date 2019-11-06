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
                       UserName = model.UserName,
                       PhoneNumber = model.PhoneNumber,
                       Roles = model.Roles,
                       Claims = model.Claims,
                       UserId = model.UserId,
                       Email = model.Email
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
                       Enabled = model.Enabled
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

        #endregion
    }
}