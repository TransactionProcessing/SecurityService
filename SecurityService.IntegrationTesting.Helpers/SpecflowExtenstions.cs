namespace SecurityService.IntegrationTesting.Helpers{
    using DataTransferObjects.Requests;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.IntegrationTesting;
    using Shouldly;
    using System.Collections.Generic;
    using DataTransferObjects;
    using TechTalk.SpecFlow;
    using static IdentityModel.OidcConstants;
    using static System.Formats.Asn1.AsnWriter;

    public static class SpecflowExtensions{
        public static List<CreateApiScopeRequest> ToCreateApiScopeRequests(this TableRows tableRows){
            List<CreateApiScopeRequest> requests = new List<CreateApiScopeRequest>();
            foreach (TableRow tableRow in tableRows){
                CreateApiScopeRequest createApiScopeRequest = new CreateApiScopeRequest{
                                                                                           Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                                           Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                           DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                                       };
                requests.Add(createApiScopeRequest);
            }

            return requests;
        }

        public static List<CreateApiResourceRequest> ToCreateApiResourceRequests(this TableRows tableRows, Guid? testId = null){
            List<CreateApiResourceRequest> requests = new List<CreateApiResourceRequest>();
            foreach (TableRow tableRow in tableRows){
                String resourceName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                String displayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");
                String description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                String secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret");
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

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

        public static List<CreateClientRequest> ToCreateClientRequests(this TableRows tableRows, Guid? testId = null, Int32? testUIPort = null){
            List<CreateClientRequest> requests = new List<CreateClientRequest>();

            foreach (TableRow tableRow in tableRows){
                String clientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId");
                String clientName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                String clientDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                String secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret");
                String allowedScopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String allowedGrantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");
                String redirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "RedirectUris");
                String postLogoutRedirectUris = SpecflowTableHelper.GetStringRowValue(tableRow, "PostLogoutRedirectUris");
                String clientUri = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientUri");
                Boolean requireConsent = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireConsent");
                Boolean allowOfflineAccess = SpecflowTableHelper.GetBooleanValue(tableRow, "AllowOfflineAccess");

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

        public static List<ApiResourceDetails> ToApiResourceDetails(this TableRows tableRows){
            List<ApiResourceDetails> result = new List<ApiResourceDetails>();
            foreach (TableRow tableRow in tableRows){
                ApiResourceDetails details = new ApiResourceDetails();
                details.Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                details.Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                details.DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");

                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

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

        public static List<ApiScopeDetails> ToApiScopeDetails(this TableRows tableRows){
            List<ApiScopeDetails> result = new List<ApiScopeDetails>();

            foreach (TableRow tableRow in tableRows){
                ApiScopeDetails scopeDetails = new ApiScopeDetails();
                scopeDetails.Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                scopeDetails.Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                scopeDetails.DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");
                result.Add(scopeDetails);
            }

            return result;
        }

        public static List<ClientDetails> ToClientDetails(this TableRows tableRows){
            List<ClientDetails> result = new List<ClientDetails>();

            foreach (TableRow tableRow in tableRows){
                ClientDetails clientDetails = new ClientDetails();

                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String grantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "GrantTypes");

                clientDetails.ClientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId");
                clientDetails.ClientDescription = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                clientDetails.ClientName = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
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

        public static List<CreateIdentityResourceRequest> ToCreateIdentityResourceRequest(this TableRows tableRows){
            List<CreateIdentityResourceRequest> result = new List<CreateIdentityResourceRequest>();
            foreach (TableRow tableRow in tableRows){
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");
                CreateIdentityResourceRequest createIdentityResourceRequest = new CreateIdentityResourceRequest{
                                                                                                                   Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                                                                                                                   Claims = string.IsNullOrEmpty(userClaims) ? null : userClaims.Split(",").ToList(),
                                                                                                                   Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                                                                                                                   DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                                                                                                               };
                result.Add(createIdentityResourceRequest);
            }

            return result;
        }

        public static List<IdentityResourceDetails> ToIdentityResourceDetails(this TableRows tableRows){
            List<IdentityResourceDetails> result = new List<IdentityResourceDetails>();

            foreach (TableRow tableRow in tableRows){
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                IdentityResourceDetails identityResourceDetails = new IdentityResourceDetails();

                identityResourceDetails.Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description");
                identityResourceDetails.Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name");
                identityResourceDetails.DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");

                if (string.IsNullOrEmpty(userClaims) == false){
                    identityResourceDetails.Claims = userClaims.SplitString();
                }

                result.Add(identityResourceDetails);
            }

            return result;
        }

        public static List<CreateRoleRequest> ToCreateRoleRequests(this TableRows tableRows){
            List<CreateRoleRequest> requests = new List<CreateRoleRequest>();

            foreach (TableRow tableRow in tableRows){
                CreateRoleRequest request = new CreateRoleRequest{
                                                                     RoleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name")
                                                                 };
                requests.Add(request);
            }

            return requests;
        }

        public static List<RoleDetails> ToRoleDetails(this TableRows tableRows){
            List<RoleDetails> requests = new List<RoleDetails>();

            foreach (TableRow tableRow in tableRows){
                RoleDetails roleDetails = new RoleDetails(){
                                                               RoleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Role Name")
                                                           };
                requests.Add(roleDetails);
            }

            return requests;
        }

        public static List<CreateUserRequest> ToCreateUserRequests(this TableRows tableRows){
            List<CreateUserRequest> requests = new List<CreateUserRequest>();

            foreach (TableRow tableRow in tableRows){
                Dictionary<String, String> userClaims = null;
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                if (string.IsNullOrEmpty(claims) == false){
                    userClaims = new Dictionary<String, String>();
                    String[] claimList = claims.Split(",");
                    foreach (String claim in claimList){
                        // Split into claim name and value
                        String[] c = claim.Split(":");
                        userClaims.Add(c[0], c[1]);
                    }
                }

                String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");

                CreateUserRequest createUserRequest = new CreateUserRequest{
                                                                               EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address"),
                                                                               FamilyName = SpecflowTableHelper.GetStringRowValue(tableRow, "Family Name"),
                                                                               GivenName = SpecflowTableHelper.GetStringRowValue(tableRow, "Given Name"),
                                                                               PhoneNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number"),
                                                                               MiddleName = SpecflowTableHelper.GetStringRowValue(tableRow, "Middle name"),
                                                                               Claims = userClaims,
                                                                               Roles = string.IsNullOrEmpty(roles) ? null : roles.Split(",").ToList(),
                                                                               Password = SpecflowTableHelper.GetStringRowValue(tableRow, "Password")
                                                                           };

                requests.Add(createUserRequest);
            }

            return requests;
        }

        public static List<UserDetails> ToUserDetails(this TableRows tableRows){

            List<UserDetails> userDetailsList = new List<UserDetails>();

            foreach (TableRow tableRow in tableRows){
                UserDetails userDetails = new UserDetails();

                userDetails.EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address");
                userDetails.PhoneNumber =
                    SpecflowTableHelper.GetStringRowValue(tableRow, "Phone Number");
                userDetails.UserName = SpecflowTableHelper.GetStringRowValue(tableRow, "Email Address");


                Dictionary<String, String> userClaims = new Dictionary<String, String>();
                String claims = SpecflowTableHelper.GetStringRowValue(tableRow, "Claims");
                String[] claimList = claims.Split(",");
                foreach (String claim in claimList){
                    // Split into claim name and value
                    String[] c = claim.Split(":");
                    userClaims.Add(c[0].Trim(), c[1].Trim());
                }

                userDetails.Claims = userClaims;

                String roles = SpecflowTableHelper.GetStringRowValue(tableRow, "Roles");


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