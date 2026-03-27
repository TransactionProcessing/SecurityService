namespace SecurityService.DataTransferObjects.Responses
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateRoleResponse
    {
        /// <summary>
        /// Gets or sets the role identifier.
        /// </summary>
        /// <value>
        /// The role identifier.
        /// </value>
        [JsonProperty("role_id")]
        public Guid RoleId { get; set; }
    }
}