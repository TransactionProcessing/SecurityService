namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;

    public class TestingContext
    {
        public DockerHelper DockerHelper { get; set; }

        public Dictionary<String, Guid> Users;

        public List<String> Clients;

        public List<String> ApiResources;

        public TestingContext()
        {
            this.Users = new Dictionary<String, Guid>();
            this.Clients=new List<String>();
            this.ApiResources=new List<String>();
        }
    }
}