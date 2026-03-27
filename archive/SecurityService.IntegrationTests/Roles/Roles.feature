@base @roles
Feature: Roles

@PRTest
Scenario: Get Roles
	Given I create the following roles
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |
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
