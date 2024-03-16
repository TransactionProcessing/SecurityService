namespace SecurityService.IntegrationTesting.Helpers{
    using DataTransferObjects.Requests;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.IntegrationTesting;
    using Shouldly;
    using System.Collections.Generic;
    using DataTransferObjects;
    using Reqnroll;
    using static IdentityModel.OidcConstants;
    using static System.Formats.Asn1.AsnWriter;

    public static class ReqnrollExtensions{
        public static List<CreateApiScopeRequest> ToCreateApiScopeRequests(this DataTableRows tableRows){
            List<CreateApiScopeRequest> requests = new List<CreateApiScopeRequest>();
            foreach (DataTableRow tableRow in tableRows){
                CreateApiScopeRequest createApiScopeRequest = new CreateApiScopeRequest{
                                                                                           Name = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                                           Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                           DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                                       };
                requests.Add(createApiScopeRequest);
            }

            return requests;
        }

        public static List<CreateApiResourceRequest> ToCreateApiResourceRequests(this DataTableRows tableRows, Guid? testId = null){
            List<CreateApiResourceRequest> requests = new List<CreateApiResourceRequest>();
            foreach (DataTableRow tableRow in tableRows){
                String resourceName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                String displayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName");
                String description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                String secret = ReqnrollTableHelper.GetStringRowValue(tableRow, "Secret");
                String scopes = ReqnrollTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");

                if (testId.HasValue){
                    scopes = scopes.Replace("[id]", testId.Value.ToString("N"));
                    resourceName = resourceName.Replace("[id]", testId.Value.ToString("N"));
                }

                CreateApiResourceRequest createApiResourceRequest = new CreateApiResourceRequest{
                                                                                                    Description = description,
                                                                                                    DisplayName = displayName,
                                                                                                    Name = resourceName,
                                                                                                    Scopes = new List<String>(),
                                                                                                    Secret = secret,
                                                                                                    UserClaims = new List<String>()
                                                                                                };
                createApiResourceRequest.Scopes = scopes.SplitString();
                createApiResourceRequest.UserClaims = userClaims.SplitString();

                requests.Add(createApiResourceRequest);
            }

            return requests;
        }

        public static List<CreateClientRequest> ToCreateClientRequests(this DataTableRows tableRows, Guid? testId = null, Int32? testUIPort = null){
            List<CreateClientRequest> requests = new List<CreateClientRequest>();

            foreach (DataTableRow tableRow in tableRows){
                String clientId = ReqnrollTableHelper.GetStringRowValue(tableRow, "ClientId");
                String clientName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                String clientDescription = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                String secret = ReqnrollTableHelper.GetStringRowValue(tableRow, "Secret");
                String allowedScopes = ReqnrollTableHelper.GetStringRowValue(tableRow, "Scopes");
                String allowedGrantTypes = ReqnrollTableHelper.GetStringRowValue(tableRow, "GrantTypes");
                String redirectUris = ReqnrollTableHelper.GetStringRowValue(tableRow, "RedirectUris");
                String postLogoutRedirectUris = ReqnrollTableHelper.GetStringRowValue(tableRow, "PostLogoutRedirectUris");
                String clientUri = ReqnrollTableHelper.GetStringRowValue(tableRow, "ClientUri");
                Boolean requireConsent = ReqnrollTableHelper.GetBooleanValue(tableRow, "RequireConsent");
                Boolean allowOfflineAccess = ReqnrollTableHelper.GetBooleanValue(tableRow, "AllowOfflineAccess");

                if (testId.HasValue){
                    allowedScopes = allowedScopes.Replace("[id]", testId.Value.ToString("N"));
                    clientId = clientId.Replace("[id]", testId.Value.ToString("N"));
                }

                if (testUIPort.HasValue){
                    clientUri = clientUri.Replace("[url]", "localhost");
                    clientUri = clientUri.Replace("[port]", testUIPort.ToString());

                    redirectUris = redirectUris.Replace("[url]", "localhost");
                    redirectUris = redirectUris.Replace("[port]", testUIPort.ToString());
                    postLogoutRedirectUris = postLogoutRedirectUris.Replace("[url]", "localhost");
                    postLogoutRedirectUris = postLogoutRedirectUris.Replace("[port]", testUIPort.ToString());
                }

                CreateClientRequest createClientRequest = new CreateClientRequest{
                                                                                     Secret = secret,
                                                                                     AllowedGrantTypes = new List<String>(),
                                                                                     AllowedScopes = new List<String>(),
                                                                                     ClientDescription = clientDescription,
                                                                                     ClientId = clientId,
                                                                                     ClientName = clientName,
                                                                                     RequireConsent = requireConsent, 
                                                                                     AllowOfflineAccess = allowOfflineAccess,
                                                                                     ClientUri = clientUri
                };
                createClientRequest.AllowedScopes = allowedScopes.SplitString();
                createClientRequest.AllowedGrantTypes = allowedGrantTypes.SplitString();
                createClientRequest.ClientRedirectUris = redirectUris.SplitString();
                createClientRequest.ClientPostLogoutRedirectUris = postLogoutRedirectUris.SplitString();

                requests.Add(createClientRequest);
            }

            return requests;
        }

        public static List<ApiResourceDetails> ToApiResourceDetails(this DataTableRows tableRows){
            List<ApiResourceDetails> result = new List<ApiResourceDetails>();
            foreach (DataTableRow tableRow in tableRows){
                ApiResourceDetails details = new ApiResourceDetails();
                details.Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                details.Name = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                details.DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName");

                String scopes = ReqnrollTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");

                if (string.IsNullOrEmpty(scopes) == false){
                    //List<String> splitScopes = scopes.Split(",").ToList();
                    details.Scopes = scopes.SplitString();

                    //splitScopes.ForEach(a => { details.Scopes.Add(a.Trim()); });
                }

                if (string.IsNullOrEmpty(userClaims) == false){
                    //List<String> splitUserClaims = userClaims.Split(",").ToList();

                    //splitUserClaims.ForEach(a => { details.UserClaims.Add(a.Trim());});
                    details.UserClaims = userClaims.SplitString();
                }

                result.Add(details);
            }

            return result;
        }

        public static List<ApiScopeDetails> ToApiScopeDetails(this DataTableRows tableRows){
            List<ApiScopeDetails> result = new List<ApiScopeDetails>();

            foreach (DataTableRow tableRow in tableRows){
                ApiScopeDetails scopeDetails = new ApiScopeDetails();
                scopeDetails.Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                scopeDetails.Name = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                scopeDetails.DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName");
                result.Add(scopeDetails);
            }

            return result;
        }

        public static List<ClientDetails> ToClientDetails(this DataTableRows tableRows){
            List<ClientDetails> result = new List<ClientDetails>();

            foreach (DataTableRow tableRow in tableRows){
                ClientDetails clientDetails = new ClientDetails();

                String scopes = ReqnrollTableHelper.GetStringRowValue(tableRow, "Scopes");
                String grantTypes = ReqnrollTableHelper.GetStringRowValue(tableRow, "GrantTypes");

                clientDetails.ClientId = ReqnrollTableHelper.GetStringRowValue(tableRow, "ClientId");
                clientDetails.ClientDescription = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                clientDetails.ClientName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                if (string.IsNullOrEmpty(scopes) == false){
                    clientDetails.AllowedScopes = scopes.SplitString();
                }

                if (string.IsNullOrEmpty(grantTypes) == false){
                    clientDetails.AllowedGrantTypes = grantTypes.SplitString();
                }
                result.Add(clientDetails);
            }

            return result;
        }

        public static List<CreateIdentityResourceRequest> ToCreateIdentityResourceRequest(this DataTableRows tableRows){
            List<CreateIdentityResourceRequest> result = new List<CreateIdentityResourceRequest>();
            foreach (DataTableRow tableRow in tableRows){
                String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");
                CreateIdentityResourceRequest createIdentityResourceRequest = new CreateIdentityResourceRequest{
                                                                                                                   Name = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                                                                   Claims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                                                                   Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                                                   DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                                                               };
                result.Add(createIdentityResourceRequest);
            }

            return result;
        }

        public static List<IdentityResourceDetails> ToIdentityResourceDetails(this DataTableRows tableRows){
            List<IdentityResourceDetails> result = new List<IdentityResourceDetails>();

            foreach (DataTableRow tableRow in tableRows){
                String userClaims = ReqnrollTableHelper.GetStringRowValue(tableRow, "UserClaims");

                IdentityResourceDetails identityResourceDetails = new IdentityResourceDetails();

                identityResourceDetails.Description = ReqnrollTableHelper.GetStringRowValue(tableRow, "Description");
                identityResourceDetails.Name = ReqnrollTableHelper.GetStringRowValue(tableRow, "Name");
                identityResourceDetails.DisplayName = ReqnrollTableHelper.GetStringRowValue(tableRow, "DisplayName");

                if (string.IsNullOrEmpty(userClaims) == false){
                    identityResourceDetails.Claims = userClaims.SplitString();
                }

                result.Add(identityResourceDetails);
            }

            return result;
        }

        public static List<CreateRoleRequest> ToCreateRoleRequests(this DataTableRows tableRows){
            List<CreateRoleRequest> requests = new List<CreateRoleRequest>();

            foreach (DataTableRow tableRow in tableRows){
                CreateRoleRequest request = new CreateRoleRequest{
                                                                     RoleName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Role Name")
                                                                 };
                requests.Add(request);
            }

            return requests;
        }

        public static List<RoleDetails> ToRoleDetails(this DataTableRows tableRows){
            List<RoleDetails> requests = new List<RoleDetails>();

            foreach (DataTableRow tableRow in tableRows){
                RoleDetails roleDetails = new RoleDetails(){
                                                               RoleName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Role Name")
                                                           };
                requests.Add(roleDetails);
            }

            return requests;
        }

        public static List<CreateUserRequest> ToCreateUserRequests(this DataTableRows tableRows){
            List<CreateUserRequest> requests = new List<CreateUserRequest>();

            foreach (DataTableRow tableRow in tableRows){
                Dictionary<String, String> userClaims = null;
                String claims = ReqnrollTableHelper.GetStringRowValue(tableRow, "Claims");
                if (string.IsNullOrEmpty(claims) == false){
                    userClaims = new Dictionary<String, String>();
                    String[] claimList = claims.Split(",");
                    foreach (String claim in claimList){
                        // Split into claim name and value
                        String[] c = claim.Split(":");
                        userClaims.Add(c[0], c[1]);
                    }
                }

                String roles = ReqnrollTableHelper.GetStringRowValue(tableRow, "Roles");

                CreateUserRequest createUserRequest = new CreateUserRequest{
                                                                               EmailAddress = ReqnrollTableHelper.GetStringRowValue(tableRow, "Email Address"),
                                                                               FamilyName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Family Name"),
                                                                               GivenName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Given Name"),
                                                                               PhoneNumber = ReqnrollTableHelper.GetStringRowValue(tableRow, "Phone Number"),
                                                                               MiddleName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Middle name"),
                                                                               Claims = userClaims,
                                                                               Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").ToList(),
                                                                               Password = ReqnrollTableHelper.GetStringRowValue(tableRow, "Password")
                                                                           };

                requests.Add(createUserRequest);
            }

            return requests;
        }

        public static List<UserDetails> ToUserDetails(this DataTableRows tableRows){

            List<UserDetails> userDetailsList = new List<UserDetails>();

            foreach (DataTableRow tableRow in tableRows){
                UserDetails userDetails = new UserDetails();

                userDetails.EmailAddress = ReqnrollTableHelper.GetStringRowValue(tableRow, "Email Address");
                userDetails.PhoneNumber =
                    ReqnrollTableHelper.GetStringRowValue(tableRow, "Phone Number");
                userDetails.UserName = ReqnrollTableHelper.GetStringRowValue(tableRow, "Email Address");


                Dictionary<String, String> userClaims = new Dictionary<String, String>();
                String claims = ReqnrollTableHelper.GetStringRowValue(tableRow, "Claims");
                String[] claimList = claims.Split(",");
                foreach (String claim in claimList){
                    // Split into claim name and value
                    String[] c = claim.Split(":");
                    userClaims.Add(c[0].Trim(), c[1].Trim());
                }

                userDetails.Claims = userClaims;

                String roles = ReqnrollTableHelper.GetStringRowValue(tableRow, "Roles");


                if (string.IsNullOrEmpty(roles) == false){
                    userDetails.Roles = roles.SplitString();
                }
                userDetailsList.Add(userDetails);
            }
            return userDetailsList;
        }

        private static List<String> SplitString(this String stringToSplit){
            List<string> result = new List<string>();

            List<String> splitString = stringToSplit.Split(",").ToList();
            foreach (String split in splitString){
                result.Add(split.Trim());
            }
            return result;
        }
    }
}