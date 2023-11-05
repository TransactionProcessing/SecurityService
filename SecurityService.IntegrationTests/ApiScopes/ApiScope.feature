@base @apiscopes
Feature: ApiScope

@PRTest
Scenario: Get Api Scopes
	Given I create the following api scopes
	| Name       | DisplayName | Description           |
	| testscope1 | Test Scope1 | A scope for testing 1 |
	| testscope2 | Test Scope2 | A scope for testing 2 |
	When I get the api scope with name 'testscope1' the api scope details are returned as follows
	| Name       | DisplayName | Description              |
	| testscope1 | Test Scope1 | A scope for testing 1 |
	When I get the api scope with name 'testscope2' the api scope details are returned as follows
	| Name       | DisplayName | Description              |
	| testscope2 | Test Scope2 | A scope for testing 2 |
	When I get the api scopes 2 api scope details are returned as follows
	| Name          | DisplayName    | Description              |
	| testscope1 | Test Scope1 | A scope for testing 1 |
	| testscope2 | Test Scope2 | A scope for testing 2 |