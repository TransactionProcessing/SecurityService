@base @apiresources
Feature: ApiResource

@PRTest
Scenario: Get Api Resources
Given I create the following api resources
| Name          | DisplayName    | Description              | Secret  | Scopes        | UserClaims    |
| testresource1 | Test Resource1 | A resource for testing 1 | secret1 | Scope1,Scope2 | Claim1,Claim2 |
| testresource2 | Test Resource2 | A resource for testing 2 | secret2 | Scope1,Scope2 | Claim1,Claim2 |
When I get the api resource with name 'testresource1' the api resource details are returned as follows
| Name          | DisplayName    | Description              | Scopes        | UserClaims    |
| testresource1 | Test Resource1 | A resource for testing 1 | Scope1,Scope2 | Claim1,Claim2 |
When I get the api resource with name 'testresource2' the api resource details are returned as follows
| Name          | DisplayName    | Description              | Scopes        | UserClaims    |
| testresource2 | Test Resource2 | A resource for testing 2 | Scope1,Scope2 | Claim1,Claim2 |
When I get the api resources 2 api resource details are returned as follows
| Name          | DisplayName    | Description              | Scopes        | UserClaims    |
| testresource1 | Test Resource1 | A resource for testing 1 | Scope1,Scope2 | Claim1,Claim2 |
| testresource2 | Test Resource2 | A resource for testing 2 | Scope1,Scope2 | Claim1,Claim2 |