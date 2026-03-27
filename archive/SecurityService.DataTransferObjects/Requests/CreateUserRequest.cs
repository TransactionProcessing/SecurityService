namespace SecurityService.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class CreateUserRequest
    {
        [JsonProperty("email_address")]
        public String EmailAddress { get; set; }

        [JsonProperty("phone_number")]
        public String PhoneNumber { get; set; }

        [JsonProperty("claims")]
        public Dictionary<String, String> Claims { get; set; }

        [JsonProperty("roles")]
        public List<String> Roles { get; set; }
        
        [JsonProperty("given_name")]
        public String GivenName { get; set; }
        
        [JsonProperty("middle_name")]
        public String MiddleName { get; set; }
        
        [JsonProperty("family_name")]
        public String FamilyName { get; set; }

        [JsonProperty("password")]
        public String Password { get; set; }
    }
}
