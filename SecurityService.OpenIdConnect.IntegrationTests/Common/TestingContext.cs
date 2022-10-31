namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using Shared.Logger;

    public class TestingContext
    {
        public DockerHelper DockerHelper { get; set; }

        public Dictionary<String, Guid> Users;
        public Dictionary<String, Guid> Roles;

        public List<String> Clients;

        public List<String> ApiResources;

        public List<String> IdentityResources;

        public TokenResponse TokenResponse;

        public NlogLogger Logger { get; set; }

        public String ResetPasswordLink { get; set; }
        public String ConfirmEmailAddressLink { get; set; }
        public String EmailAddress { get; set; }
        public String Password { get; set; }

        public TestingContext()
        {
            this.Users = new Dictionary<String, Guid>();
            this.Roles= new Dictionary<String, Guid>();
            this.Clients=new List<String>();
            this.ApiResources=new List<String>();
            this.IdentityResources= new List<String>();
        }
    }
}