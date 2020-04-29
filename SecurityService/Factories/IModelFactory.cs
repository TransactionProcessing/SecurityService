using UserDetailsDto = SecurityService.DataTransferObjects.Responses.UserDetails;
using RoleDetailsDto = SecurityService.DataTransferObjects.Responses.RoleDetails;
using UserDetailsModel = SecurityService.Models.UserDetails;
using RoleDetailsModel = SecurityService.Models.RoleDetails;

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
        RoleDetailsDto ConvertFrom(RoleDetailsModel model);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        List<RoleDetailsDto> ConvertFrom(List<RoleDetailsModel> model);

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

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        ApiResourceDetails ConvertFrom(ApiResource model);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        List<ApiResourceDetails> ConvertFrom(List<ApiResource> model);

        #endregion
    }
}