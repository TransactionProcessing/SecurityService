using Shared.Results;
using SimpleResults;

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
    using static Shared.Results.ResultHelpers;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClientProxyBase" />
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

        public async Task<Result> CreateApiResource(CreateApiResourceRequest createApiResourceRequest,
                                                    CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if(result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating api resource {createApiResourceRequest.Name}.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                 CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/apiscopes");

            try
            {
                String requestSerialised = JsonConvert.SerializeObject(createApiScopeRequest);

                StringContent httpContent = new StringContent(requestSerialised, Encoding.UTF8, "application/json");

                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.PostAsync(requestUri, httpContent, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating api scope {createApiScopeRequest.Name}.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreateClient(CreateClientRequest createClientRequest,
                                               CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating client {createClientRequest.ClientId}.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                         CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating identity resource {createIdentityResourceRequest.DisplayName}.", ex);

                throw exception;
            }
        }

        public async Task<Result> CreateRole(CreateRoleRequest createRoleRequest,
                                             CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating role {createRoleRequest.RoleName}.", ex);

                throw exception;
            }
        }

        /// <summary>
        /// Creates the user.
        /// </summary>
        /// <param name="createUserRequest">The create user request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result> CreateUser(CreateUserRequest createUserRequest,
                                                                 CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success();
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error creating user {createUserRequest.EmailAddress}.", ex);

                throw exception;
            }
        }

        public async Task<Result<ApiResourceDetails>> GetApiResource(String apiResourceName,
                                                                     CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/apiresources/{apiResourceName}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<ApiResourceDetails> responseData = HandleResponseContent<ApiResourceDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting api resource {apiResourceName}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<ApiResourceDetails>>> GetApiResources(CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/apiresources");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<ApiResourceDetails>> responseData = HandleResponseContent<List<ApiResourceDetails>>(result.Data);
                
                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting api resources.", ex);

                throw exception;
            }
        }

        public async Task<Result<ApiScopeDetails>> GetApiScope(String apiScopeName,
                                                               CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/apiscopes/{apiScopeName}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<ApiScopeDetails> responseData = HandleResponseContent<ApiScopeDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting api scope {apiScopeName}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<ApiScopeDetails>>> GetApiScopes(CancellationToken cancellationToken)
        {
            List<ApiScopeDetails> response = null;
            String requestUri = this.BuildRequestUrl("/api/apiscopes");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<ApiScopeDetails>> responseData = HandleResponseContent<List<ApiScopeDetails>>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting api scopes.", ex);

                throw exception;
            }
        }

        public async Task<Result<ClientDetails>> GetClient(String clientId,
                                                           CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/clients/{clientId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<ClientDetails> responseData = HandleResponseContent<ClientDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting client {clientId}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<ClientDetails>>> GetClients(CancellationToken cancellationToken)
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<ClientDetails>> responseData =
                    HandleResponseContent<List<ClientDetails>>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting clients.", ex);

                throw exception;
            }
        }

        public async Task<Result<IdentityResourceDetails>> GetIdentityResource(String identityResourceName,
                                                                               CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/identityresources/{identityResourceName}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<IdentityResourceDetails> responseData =
                    HandleResponseContent<IdentityResourceDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting identity resource {identityResourceName}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<IdentityResourceDetails>>> GetIdentityResources(CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/identityresources");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<IdentityResourceDetails>> responseData =
                    HandleResponseContent<List<IdentityResourceDetails>>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting identity resources.", ex);

                throw exception;
            }
        }

        public async Task<Result<RoleDetails>> GetRole(Guid roleId,
                                                       CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/roles/{roleId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<RoleDetails> responseData =
                    HandleResponseContent<RoleDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error get role with Id {roleId}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<RoleDetails>>> GetRoles(CancellationToken cancellationToken)
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<RoleDetails>> responseData =
                    HandleResponseContent<List<RoleDetails>>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error get roles.", ex);

                throw exception;
            }
        }

        public async Task<Result<TokenResponse>> GetToken(String username,
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

            return await this.GetToken(queryString.ToString(), cancellationToken);
        }

        public async Task<Result<TokenResponse>> GetToken(String clientId,
                                                  String clientSecret,
                                                  CancellationToken cancellationToken)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("grant_type=client_credentials");
            queryString.Append($"&client_id={clientId}");
            queryString.Append($"&client_secret={clientSecret}");

            return await this.GetToken(queryString.ToString(), cancellationToken);
        }

        public async Task<Result<TokenResponse>> GetToken(String clientId,
                                                  String clientSecret,
                                                  String refreshToken,
                                                  CancellationToken cancellationToken)
        {
            StringBuilder queryString = new StringBuilder();

            queryString.Append("grant_type=client_credentials");
            queryString.Append($"&client_id={clientId}");
            queryString.Append($"&client_secret={clientSecret}");
            queryString.Append($"&refresh_token={refreshToken}");

            return await this.GetToken(queryString.ToString(), cancellationToken);
        }

        public async Task<Result<UserDetails>> GetUser(Guid userId,
                                                       CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/users/{userId}");

            try
            {
                // Add the access token to the client headers
                //this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<UserDetails> responseData =
                    HandleResponseContent<UserDetails>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error get user with Id {userId}.", ex);

                throw exception;
            }
        }

        public async Task<Result<List<UserDetails>>> GetUsers(String userName,
                                                              CancellationToken cancellationToken)
        {
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                ResponseData<List<UserDetails>> responseData =
                    HandleResponseContent<List<UserDetails>>(result.Data);

                return Result.Success(responseData.Data);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error get users.", ex);

                throw exception;
            }
        }

        private String BuildRequestUrl(String route)
        {
            if (string.IsNullOrEmpty(this.BaseAddress))
            {
                this.BaseAddress = this.BaseAddressResolver("SecurityService");
            }

            String requestUri = $"{this.BaseAddress}{route}";
            return requestUri;
        }

        private async Task<Result<TokenResponse>> GetToken(String tokenRequest,
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
                Result<String> result = await this.HandleResponseX(httpResponse, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                // call was successful so now deserialise the body to the response object
                TokenResponse responseData = TokenResponse.Create(result.Data);

                return Result.Success(responseData);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting token.", ex);

                throw exception;
            }
        }

        #endregion
    }
}