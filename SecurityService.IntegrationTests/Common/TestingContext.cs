using SecurityService.DataTransferObjects;

namespace SecurityService.IntergrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Shared.Logger;

    /// <summary>
    /// 
    /// </summary>
    public class TestingContext
    {
        /// <summary>
        /// Gets or sets the docker helper.
        /// </summary>
        /// <value>
        /// The docker helper.
        /// </value>
        public DockerHelper DockerHelper { get; set; }

        /// <summary>
        /// The users
        /// </summary>
        public Dictionary<String, String> Users;
        /// <summary>
        /// The roles
        /// </summary>
        public Dictionary<String, String> Roles;

        /// <summary>
        /// The clients
        /// </summary>
        public List<String> Clients;

        /// <summary>
        /// The API resources
        /// </summary>
        public List<String> ApiResources;

        /// <summary>
        /// The API scopes
        /// </summary>
        public List<String> ApiScopes;

        /// <summary>
        /// The identity resources
        /// </summary>
        public List<String> IdentityResources;

        /// <summary>
        /// The token response
        /// </summary>
        public TokenResponse TokenResponse;
        public String AccessToken;

        public NlogLogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestingContext"/> class.
        /// </summary>
        public TestingContext()
        {
            this.Users = new Dictionary<String, String>();
            this.Roles= new Dictionary<String, String>();
            this.Clients=new List<String>();
            this.ApiScopes = new List<String>();
            this.ApiResources=new List<String>();
            this.IdentityResources = new List<String>();
        }
    }
}