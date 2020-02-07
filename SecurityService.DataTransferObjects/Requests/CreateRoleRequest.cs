using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.DataTransferObjects.Requests
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateRoleRequest
    {
        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        /// <value>
        /// The name of the role.
        /// </value>
        [Required]
        [JsonProperty("role_name")]
        public String RoleName { get; set; }
    }
}
