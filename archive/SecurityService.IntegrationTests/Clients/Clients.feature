@base @clients
Feature: Clients

@PRTest
Scenario: Get Clients
	Given I create the following clients
	| ClientId    | Name          | Description          | Secret  | Scopes         | GrantTypes         | RedirectUris                 | PostLogoutRedirectUris        | RequireConsent |
	| testclient1 | Test Client 1 | A test client 1      | secret1 | Scope1, Scope2 | client_credentials |                              |                               |                |
	| testclient2 | Test Client 2 | A second test client | Secret2 | Scope1, Scope2 | hybrid             | http://localhost/signin-oidc | http://localhost/signout-oidc | true           |

	When I get the client with client id 'testclient1' the client details are returned as follows
	| ClientId    | Name          | Description     | Scopes         | GrantTypes         | RedirectUris | PostLogoutRedirectUris | RequireConsent |
	| testclient1 | Test Client 1 | A test client 1 | Scope1, Scope2 | client_credentials |              |                        |                |

	When I get the client with client id 'testclient2' the client details are returned as follows
	| ClientId    | Name          | Description          | Scopes         | GrantTypes | RedirectUris                 | PostLogoutRedirectUris        | RequireConsent |
	| testclient2 | Test Client 2 | A second test client | Scope1, Scope2 | hybrid     | http://localhost/signin-oidc | http://localhost/signout-oidc | true           |


	When I get the clients 2 clients details are returned as follows
	| ClientId    | Name          | Description          | Scopes         | GrantTypes         | RedirectUris                 | PostLogoutRedirectUris        | RequireConsent |
	| testclient1 | Test Client 1 | A test client 1      | Scope1, Scope2 | client_credentials |                              |                               |                |
	| testclient2 | Test Client 2 | A second test client | Scope1, Scope2 | hybrid             | http://localhost/signin-oidc | http://localhost/signout-oidc | true           |