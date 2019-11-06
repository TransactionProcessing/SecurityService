using UserDetailsDto = SecurityService.DataTransferObjects.Responses.UserDetails;
using UserDetailsModel = SecurityService.Models.UserDetails;

namespace SecurityService.Factories
{
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using IdentityServer4.Models;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        UserDetailsDto ConvertFrom(UserDetailsModel model);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        List<UserDetailsDto> ConvertFrom(List<UserDetailsModel> model);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        ClientDetails ConvertFrom(Client model);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        List<ClientDetails> ConvertFrom(List<Client> model);

        #endregion
    }
}