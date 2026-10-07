@base @roles
Feature: Roles

@PRTest
Scenario: Get Roles
	Given I create the following roles
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |

@mfa
Scenario: Require and remove MFA for a role
	When I require MFA for role 'TestRole1'
	Then the MFA policy operation succeeds
	When I require MFA for role 'TestRole1'
	Then the MFA policy operation succeeds
	When I remove MFA for role 'TestRole1'
	Then the MFA policy operation succeeds
	When I get the role with name 'TestRole1' the role details are returned as follows
	| Role Name |
	| TestRole1 |
	When I get the role with name 'TestRole2' the role details are returned as follows
	| Role Name |
	| TestRole2 |
	When I get the role with name 'TestRole3' the role details are returned as follows
	| Role Name |
	| TestRole3 |
	When I get the roles 3 roles details are returned as follows
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |
