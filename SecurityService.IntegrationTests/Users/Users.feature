@base @users @roles
Feature: Users

Background: 
	Given I create the following roles
	| Role Name |
	| TestRole1 |
	| TestRole2 |
	| TestRole3 |

@PRTest
Scenario: Get Users
	Given I create the following users
	| Email Address           | Phone Number | Given Name | Middle Name | Family Name | Claims | Roles     |
	| testuser1@testing.co.uk | 123456789    | Test       |             | User 1      |        | TestRole1 |
	| testuser2@testing.co.uk | 123456789    | Test       |             | User 2      |        | TestRole2 |
	| testuser3@testing.co.uk | 123456789    | Test       |             | User 3      |        | TestRole3 |
	When I get the user with user name 'testuser1@testing.co.uk' the user details are returned as follows
	| Email Address           | Phone Number | Given Name | Middle Name | Family Name | Claims                                                             | Roles     |
	| testuser1@testing.co.uk | 123456789    | Test       |             | User 1      | email:testuser1@testing.co.uk, given_name:Test, family_name:User 1 | TestRole1 |
	When I get the user with user name 'testuser2@testing.co.uk' the user details are returned as follows
	| Email Address           | Phone Number | Given Name | Middle Name | Family Name | Claims                                                             | Roles     |
	| testuser2@testing.co.uk | 123456789    | Test       |             | User 2      | email:testuser2@testing.co.uk, given_name:Test, family_name:User 2 | TestRole2 |
	When I get the user with user name 'testuser3@testing.co.uk' the user details are returned as follows
	| Email Address           | Phone Number | Given Name | Middle Name | Family Name | Claims                                                            | Roles     |
	| testuser3@testing.co.uk | 123456789    | Test       |             | User 3      | email:testuser3@testing.co.uk, given_name:Test, family_name:User 3 | TestRole3 |
	When I get the users 3 users details are returned as follows
	| Email Address           | Phone Number | Given Name | Middle Name | Family Name | Claims                                                             | Roles     |
	| testuser1@testing.co.uk | 123456789    | Test       |             | User 1      | email:testuser1@testing.co.uk, given_name:Test, family_name:User 1 | TestRole1 |
	| testuser2@testing.co.uk | 123456789    | Test       |             | User 2      | email:testuser2@testing.co.uk, given_name:Test, family_name:User 2 | TestRole2 |
	| testuser3@testing.co.uk | 123456789    | Test       |             | User 3      | email:testuser3@testing.co.uk, given_name:Test, family_name:User 3 | TestRole3 |
