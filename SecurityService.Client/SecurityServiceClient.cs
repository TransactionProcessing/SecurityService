namespace SecurityService.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ClientProxyBase;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClientProxyBase.ClientProxyBase" />
    /// <seealso cref="SecurityService.Client.ISecurityServiceClient" />
    public class SecurityServiceClient : ClientProxyBase, ISecurityServiceClient
    {
        #region Fields

        /// <summary>
        /// The base address
        /// </summary>
        private String BaseAddress;

        /// <summary>
        /// The base address resolver
        /// </summary>
        private readonly Func<String, String> BaseAddressResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityServiceClient" /> class.
        /// </summary>
        /// <param name="baseAddressResolver">The base address resolver.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public SecurityServiceClient(Func<String, String> baseAddressResolver,
                                     HttpClient httpClient) : base(httpClient)
        {
            this.BaseAddressResolver = baseAddressResolver;
            this.BaseAddress = baseAddressResolver("SecurityService");

            // Add the API version header
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the API resource.
        /// </summary>
        /// <param name="createApiResourceRequest">The create API resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<CreateApiResourceResponse> CreateApiResource(CreateApiResourceRequest createApiResourceRequest,
                                                                       CancellationToken cancellationToken)
        {
            CreateApiResourceResponse response = null;
            String requestUri = this.BuildRequestUrl("/api/apiresources");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createApiResourceRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<CreateApiResourceResponse>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating api resource {createApiResourceRequest.Name}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="createClientRequest">The create client request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<CreateClientResponse> CreateClient(CreateClientRequest createClientRequest,
                                                             CancellationToken cancellationToken)
        {
            CreateClientResponse response = null;
            String requestUri = this.BuildRequestUrl("/api/clients");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createClientRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<CreateClientResponse>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating client {createClientRequest.ClientId}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Creates the identity resource.
        /// </summary>
        /// <param name="createIdentityResourceRequest">The create identity resource request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<CreateIdentityResourceResponse> CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                                 CancellationToken cancellationToken)
        {
            CreateIdentityResourceResponse response = null;
            String requestUri = this.BuildRequestUrl("/api/identityresources");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createIdentityResourceRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<CreateIdentityResourceResponse>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating identity resource {createIdentityResourceRequest.DisplayName}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="createRoleRequest">The create role request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<CreateRoleResponse> CreateRole(CreateRoleRequest createRoleRequest,
                                                         CancellationToken cancellationToken)
        {
            CreateRoleResponse response = null;
            String requestUri = this.BuildRequestUrl("/api/roles");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createRoleRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<CreateRoleResponse>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating role {createRoleRequest.RoleName}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="createUserRequest">The create user request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<CreateUserResponse> CreateUser(CreateUserRequest createUserRequest,
                                                         CancellationToken cancellationToken)
        {
            CreateUserResponse response = null;
            String requestUri = this.BuildRequestUrl("/api/users");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createUserRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<CreateUserResponse>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating user {createUserRequest.EmailAddress}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the API resource.
        /// </summary>
        /// <param name="apiResourceName">Name of the API resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ApiResourceDetails> GetApiResource(String apiResourceName,
                                                             CancellationToken cancellationToken)
        {
            ApiResourceDetails response = null;
            String requestUri = this.BuildRequestUrl($"/api/apiresources/{apiResourceName}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<ApiResourceDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting api resource {apiResourceName}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the API resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<ApiResourceDetails>> GetApiResources(CancellationToken cancellationToken)
        {
            List<ApiResourceDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/apiresources");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<ApiResourceDetails>>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting api resources.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<ClientDetails> GetClient(String clientId,
                                                   CancellationToken cancellationToken)
        {
            ClientDetails response = null;
            String requestUri = this.BuildRequestUrl($"/api/clients/{clientId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<ClientDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting client {clientId}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<ClientDetails>> GetClients(CancellationToken cancellationToken)
        {
            List<ClientDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/clients");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<ClientDetails>>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting clients.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the identity resource.
        /// </summary>
        /// <param name="identityResourceName">Name of the identity resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IdentityResourceDetails> GetIdentityResource(String identityResourceName,
                                                                       CancellationToken cancellationToken)
        {
            IdentityResourceDetails response = null;
            String requestUri = this.BuildRequestUrl($"/api/identityresources/{identityResourceName}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<IdentityResourceDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting identity resource {identityResourceName}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the identity resources.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<IdentityResourceDetails>> GetIdentityResources(CancellationToken cancellationToken)
        {
            List<IdentityResourceDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/identityresources");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<IdentityResourceDetails>>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting identity resources.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        /// <param name="roleId">The role identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<RoleDetails> GetRole(Guid roleId,
                                               CancellationToken cancellationToken)
        {
            RoleDetails response = null;
            String requestUri = this.BuildRequestUrl($"/api/roles/{roleId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<RoleDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error get role with Id {roleId}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<RoleDetails>> GetRoles(CancellationToken cancellationToken)
        {
            List<RoleDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/roles");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<RoleDetails>>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error get roles.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TokenResponse> GetToken(String username,
                                                  String password,
                                                  String clientId,
                                                  String clientSecret,
                                                  CancellationToken cancellationToken)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("grant_type=password");
            queryString.Append($"&client_id={clientId}");
            queryString.Append($"&client_secret={clientSecret}");
            queryString.Append($"&username={username}");
            queryString.Append($"&password={password}");

            String token = await this.GetToken(queryString.ToString(), cancellationToken);

            return TokenResponse.Create(token);
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TokenResponse> GetToken(String clientId,
                                                  String clientSecret,
                                                  CancellationToken cancellationToken)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("grant_type=client_credentials");
            queryString.Append($"&client_id={clientId}");
            queryString.Append($"&client_secret={clientSecret}");

            String token = await this.GetToken(queryString.ToString(), cancellationToken);

            return TokenResponse.Create(token);
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TokenResponse> GetToken(String clientId,
                                                  String clientSecret,
                                                  String refreshToken,
                                                  CancellationToken cancellationToken)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("grant_type=client_credentials");
            queryString.Append($"&client_id={clientId}");
            queryString.Append($"&client_secret={clientSecret}");
            queryString.Append($"&refresh_token={refreshToken}");

            String token = await this.GetToken(queryString.ToString(), cancellationToken);

            return TokenResponse.Create(token);
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<UserDetails> GetUser(Guid userId,
                                               CancellationToken cancellationToken)
        {
            UserDetails response = null;
            String requestUri = this.BuildRequestUrl($"/api/users/{userId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<UserDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error get user with Id {userId}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Gets the users.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<UserDetails>> GetUsers(String userName,
                                                      CancellationToken cancellationToken)
        {
            List<UserDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/users");

            try
            {
                if (string.IsNullOrEmpty(userName) == false)
                {
                    requestUri = $"{requestUri}?username={userName}";
                }

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<List<UserDetails>>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error get users.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Builds the request URL.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        private String BuildRequestUrl(String route)
        {
            if (string.IsNullOrEmpty(this.BaseAddress))
            {
                this.BaseAddress = this.BaseAddressResolver("SecurityService");
            }

            String requestUri = $"{this.BaseAddress}{route}";
            return requestUri;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<String> GetToken(String tokenRequest,
                                            CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/connect/token");
            String content = null;

            try
            {
                StringContent httpContent = new StringContent(tokenRequest, Encoding.UTF8, "application/x-www-form-urlencoded");

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                content = await this.HandleResponse(httpResponse, cancellationToken);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting token.", ex);

                throw exception;
            }

            return content;
        }

        #endregion
    }
}