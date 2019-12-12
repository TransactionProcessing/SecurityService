@base @roles
Feature: Roles

Scenario: Create Role
	Given I create the following roles
	| Role Name |
	| TestRole1 |

Scenario: Get Role
	Given I create the following roles
	| Role Name |
	| TestRole1 |
	When I get the role with name 'TestRole1' the role details are returned as follows
	| Role Name |
	| TestRole1 |

@PRTest
Scenario: Get Roles
	Given I create the following roles
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |
	When I get the roles 3 roles details are returned as follows
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |
