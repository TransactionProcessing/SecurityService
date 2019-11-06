namespace SecurityService.DataTransferObjects.Requests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateClientRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the allowed grant types.
        /// </summary>
        /// <value>
        /// The allowed grant types.
        /// </value>
        public List<String> AllowedGrantTypes { get; set; }

        /// <summary>
        /// Gets or sets the allowed scopes.
        /// </summary>
        /// <value>
        /// The allowed scopes.
        /// </value>
        public List<String> AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets the client description.
        /// </summary>
        /// <value>
        /// The client description.
        /// </value>
        public String ClientDescription { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [Required]
        public String ClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <value>
        /// The name of the client.
        /// </value>
        [Required]
        public String ClientName { get; set; }

        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// <value>
        /// The secret.
        /// </value>
        [Required]
        public String Secret { get; set; }

        #endregion
    }
}