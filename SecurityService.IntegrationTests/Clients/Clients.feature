@base @clients
Feature: Clients

Scenario: Create Client
	Given I create the following clients
	| ClientId    | Name        | Description   | Secret  | Scopes         | GrantTypes         |
	| testclient1 | Test Client | A test client | secret1 | Scope1, Scope2 | client_credentials |
	
Scenario: Get Client
	Given I create the following clients
	| ClientId    | Name        | Description   | Secret  | Scopes         | GrantTypes         |
	| testclient1 | Test Client 1| A test client 1| secret1 | Scope1, Scope2 | client_credentials |
	| testclient2 | Test Client 2| A test client 2| secret2 | Scope1, Scope2 | client_credentials |
	When I get the client with client id 'testclient1' the client details are returned as follows
	| ClientId    | Name          | Description     |  Scopes         | GrantTypes         |
	| testclient1 | Test Client 1 | A test client 1 | Scope1, Scope2 | client_credentials |

@PRTest
Scenario: Get Clients
	Given I create the following clients
	| ClientId    | Name        | Description   | Secret  | Scopes         | GrantTypes         |
	| testclient1 | Test Client 1| A test client 1| secret1 | Scope1, Scope2 | client_credentials |
	| testclient2 | Test Client 2| A test client 2| secret2 | Scope1, Scope2 | client_credentials |
	When I get the clients 2 clients details are returned as follows
| ClientId    | Name          | Description     | Scopes         | GrantTypes         |
| testclient1 | Test Client 1 | A test client 1 | Scope1, Scope2 | client_credentials |
| testclient2 | Test Client 2 | A test client 2 | Scope1, Scope2 | client_credentials |