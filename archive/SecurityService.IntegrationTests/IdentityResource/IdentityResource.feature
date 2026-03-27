@base @identityresources
Feature: IdentityResource

@PRTest
Scenario: Get Identity Resources
	Given I create the following identity resources
	| Name          | DisplayName    | Description              | UserClaims    |
	| testresource1 | Test Resource1 | A resource for testing 1 | Claim1,Claim2 |
	| testresource2 | Test Resource2 | A resource for testing 2 | Claim1,Claim2 |
	When I get the identity resource with name 'testresource1' the identity resource details are returned as follows
	| Name          | DisplayName    | Description              | UserClaims    |
	| testresource1 | Test Resource1 | A resource for testing 1 | Claim1,Claim2 |
	When I get the identity resource with name 'testresource2' the identity resource details are returned as follows
	| Name          | DisplayName    | Description              | UserClaims    |
	| testresource2 | Test Resource2 | A resource for testing 2 | Claim1,Claim2 |
	When I get the identity resources 2 identity resource details are returned as follows
	| Name          | DisplayName    | Description              | UserClaims    |
	| testresource1 | Test Resource1 | A resource for testing 1 | Claim1,Claim2 |
	| testresource2 | Test Resource2 | A resource for testing 2 | Claim1,Claim2 |