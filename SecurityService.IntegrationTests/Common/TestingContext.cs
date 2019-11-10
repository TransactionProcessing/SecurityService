namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;

    public class TestingContext
    {
        public DockerHelper DockerHelper { get; set; }

        public Dictionary<String, Guid> Users;

        public TestingContext()
        {
            this.Users = new Dictionary<String, Guid>();
        }
    }
}