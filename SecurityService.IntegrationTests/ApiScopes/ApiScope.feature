@base @apiscopes
Feature: ApiScope

Scenario: Create Api Scope
	Given I create the following api scopes
	| Name      | DisplayName | Description         |
	| testscope | Test Scope  | A scope for testing |

Scenario: Get Api Scope
	Given I create the following api scopes
	| Name       | DisplayName | Description           |
	| testscope1 | Test Scope1 | A scope for testing 1 |
	| testscope2 | Test Scope2 | A scope for testing 2 |
	When I get the api scope with name 'testscope1' the api scope details are returned as follows
	| Name       | DisplayName | Description              |
	| testscope1 | Test Scope1 | A resource for testing 1 |

@PRTest
Scenario: Get Api Scopes
Given I create the following api scopes
	| Name       | DisplayName | Description           |
	| Name       | DisplayName | Description           |
	| testscope1 | Test Scope1 | A scope for testing 1 |
	| testscope2 | Test Scope2 | A scope for testing 2 |
	When I get the api scopes 2 api scope details are returned as follows
	| Name          | DisplayName    | Description              |
	| testscope1 | Test Scope1 | A resource for testing 1 |
	| testscope2 | Test Scope2 | A resource for testing 2 |