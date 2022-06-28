using System;
using System.Collections.Generic;
using System.Text;

namespace SecurityService.OpenIdConnect.IntegrationTests.Common
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using DataTransferObjects.Responses;
    using IntergrationTests.Common;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly TestingContext TestingContext;

        public SharedSteps(TestingContext testingContext)
        {
            this.TestingContext = testingContext;
        }

        [Given(@"I create the following roles")]
        public async Task GivenICreateTheFollowingRoles(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                CreateRoleRequest createRoleRequest = new CreateRoleRequest
                                                      {
                                                          RoleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name").Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N"))
                                                      };
                CreateRoleResponse createRoleResponse = await this.CreateRole(createRoleRequest, CancellationToken.None).ConfigureAwait(false);

                createRoleResponse.ShouldNotBeNull();
                createRoleResponse.RoleId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Roles.Add(createRoleRequest.RoleName, createRoleResponse.RoleId);
            }
        }

        private async Task<CreateRoleResponse> CreateRole(CreateRoleRequest createRoleRequest,
                                                          CancellationToken cancellationToken)
        {
            CreateRoleResponse createRoleResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateRole(createRoleRequest, cancellationToken).ConfigureAwait(false);
            return createRoleResponse;
        }

        private async Task<CreateApiResourceResponse> CreateApiResource(CreateApiResourceRequest createApiResourceRequest,
                                                                        CancellationToken cancellationToken)
        {
            CreateApiResourceResponse createApiResourceResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateApiResource(createApiResourceRequest, cancellationToken).ConfigureAwait(false);
            return createApiResourceResponse;
        }

        private async Task<CreateClientResponse> CreateClient(CreateClientRequest createClientRequest,
                                                              CancellationToken cancellationToken)
        {
            CreateClientResponse createClientResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateClient(createClientRequest, cancellationToken).ConfigureAwait(false);
            return createClientResponse;
        }

        [Given(@"I create the following identity resources")]
        public async Task GivenICreateTheFollowingIdentityResources(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                CreateIdentityResourceRequest createIdentityResourceRequest = new CreateIdentityResourceRequest
                                                                              {
                                                                                  Name = SpecflowTableHelper
                                                                                         .GetStringRowValue(tableRow, "Name")
                                                                                         .Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")),
                                                                                  Claims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                                  Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                  DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                              };

                await this.CreateIdentityResource(createIdentityResourceRequest, CancellationToken.None).ConfigureAwait(false);
            }
        }

        [Given(@"I create the following api resources")]
        public async Task GivenICreateTheFollowingApiResources(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");
                scopes = scopes.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N"));

                CreateApiResourceRequest createApiResourceRequest = new CreateApiResourceRequest
                                                                    {
                                                                        Secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret"),
                                                                        Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name").Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")),
                                                                        Scopes = string.IsNullOrEmpty(scopes) ? null : scopes.Split(",").ToList(),
                                                                        UserClaims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                        Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                        DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                    };
                CreateApiResourceResponse createApiResourceResponse =
                    await this.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);

                createApiResourceResponse.ShouldNotBeNull();
                createApiResourceResponse.ApiResourceName.ShouldNotBeNullOrEmpty();

                this.TestingContext.ApiResources.Add(createApiResourceResponse.ApiResourceName);
            }
        }

        private async Task CreateIdentityResource(CreateIdentityResourceRequest createIdentityResourceRequest,
                                                                             CancellationToken cancellationToken)
        {
            CreateIdentityResourceResponse createIdentityResourceResponse = null;

            List<IdentityResourceDetails> identityResourceList = await this.TestingContext.DockerHelper.SecurityServiceClient.GetIdentityResources(cancellationToken);

            if (identityResourceList == null || identityResourceList.Any() == false)
            {
                createIdentityResourceResponse = await this
                                                                                 .TestingContext.DockerHelper.SecurityServiceClient
                                                                                 .CreateIdentityResource(createIdentityResourceRequest, cancellationToken)
                                                                                 .ConfigureAwait(false);
                createIdentityResourceResponse.ShouldNotBeNull();
                createIdentityResourceResponse.IdentityResourceName.ShouldNotBeNullOrEmpty();

                this.TestingContext.IdentityResources.Add(createIdentityResourceResponse.IdentityResourceName);
            }
            else
            {
                if (identityResourceList.Where(i => i.Name == createIdentityResourceRequest.Name).Any())
                {
                    return;
                }

                createIdentityResourceResponse = await this
                                                       .TestingContext.DockerHelper.SecurityServiceClient
                                                       .CreateIdentityResource(createIdentityResourceRequest, cancellationToken)
                                                       .ConfigureAwait(false);
                createIdentityResourceResponse.ShouldNotBeNull();
                createIdentityResourceResponse.IdentityResourceName.ShouldNotBeNullOrEmpty();

                this.TestingContext.IdentityResources.Add(createIdentityResourceResponse.IdentityResourceName);
            }
        }

        [Given(@"I create the following clients")]
        public async Task GivenICreateTheFollowingClients(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the scopes
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                // Get the grant types
                String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");
                // Get the redirect uris
                String redirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "RedirectUris");
                // Get the post logout redirect uris
                String postLogoutRedirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "PostLogoutRedirectUris");

                scopes = scopes.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N"));
                redirectUris = redirectUris.Replace("[url]", "localhost");
                redirectUris = redirectUris.Replace("[port]", this.TestingContext.DockerHelper.SecurityServiceTestUIPort.ToString());
                postLogoutRedirectUris = postLogoutRedirectUris.Replace("[url]", "localhost");
                postLogoutRedirectUris = postLogoutRedirectUris.Replace("[port]", this.TestingContext.DockerHelper.SecurityServiceTestUIPort.ToString());

                String clientUri = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientUri");
                clientUri = clientUri.Replace("[url]", "localhost");
                clientUri = clientUri.Replace("[port]", this.TestingContext.DockerHelper.SecurityServiceTestUIPort.ToString());
                CreateClientRequest createClientRequest = new CreateClientRequest
                {
                    ClientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId").Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")),
                    Secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret"),
                    ClientName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                    AllowedScopes = string.IsNullOrEmpty(scopes) ? null : scopes.Split(",").ToList(),
                    AllowedGrantTypes = string.IsNullOrEmpty(grantTypes) ? null : grantTypes.Split(",").ToList(),
                    ClientRedirectUris = string.IsNullOrEmpty(redirectUris) ? null : redirectUris.Split(",").ToList(),
                    ClientPostLogoutRedirectUris = string.IsNullOrEmpty(postLogoutRedirectUris) ? null : postLogoutRedirectUris.Split(",").ToList(),
                    ClientDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                    RequireConsent = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireConsent"),
                    AllowOfflineAccess = SpecflowTableHelper.GetBooleanValue(tableRow, "AllowOfflineAccess"),
                    ClientUri = clientUri
                };

                CreateClientResponse createClientResponse = await this.CreateClient(createClientRequest, CancellationToken.None).ConfigureAwait(false);

                createClientResponse.ShouldNotBeNull();
                createClientResponse.ClientId.ShouldNotBeNullOrEmpty();

                this.TestingContext.Clients.Add(createClientResponse.ClientId);
            }
        }

        [Given(@"I create the following users")]
        public async Task GivenICreateTheFollowingUsers(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // Get the claims
                Dictionary<String, String> userClaims = null;
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                if (string.IsNullOrEmpty(claims) == false)
                {
                    userClaims = new Dictionary<String, String>();
                    String[] claimList = claims.Split(",");
                    foreach (String claim in claimList)
                    {
                        // Split into claim name and value
                        String[] c = claim.Split(":");
                        userClaims.Add(c[0], c[1]);
                    }
                }

                String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");
                roles = roles.Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N"));

                CreateUserRequest createUserRequest = new CreateUserRequest
                {
                    EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address").Replace("[id]", this.TestingContext.DockerHelper.TestId.ToString("N")),
                    FamilyName = SpecflowTableHelper.GetStringRowValue(tableRow, "Family Name"),
                    GivenName = SpecflowTableHelper.GetStringRowValue(tableRow, "Given Name"),
                    PhoneNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number"),
                    MiddleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Middle name"),
                    Claims = userClaims,
                    Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").ToList(),
                };
                CreateUserResponse createUserResponse = await this.CreateUser(createUserRequest, CancellationToken.None).ConfigureAwait(false);

                createUserResponse.ShouldNotBeNull();
                createUserResponse.UserId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Users.Add(createUserRequest.EmailAddress, createUserResponse.UserId);
            }
        }

        private async Task<CreateUserResponse> CreateUser(CreateUserRequest createUserRequest,
                                                          CancellationToken cancellationToken)
        {
            CreateUserResponse createUserResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateUser(createUserRequest, cancellationToken).ConfigureAwait(false);
            return createUserResponse;
        }
    }
}
