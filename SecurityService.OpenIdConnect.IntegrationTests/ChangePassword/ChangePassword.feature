@base @shared @userlogin @changepassword
Feature: Change Password

Background: 

	Given I create the following roles
	| Role Name  |
	| Estate |

	Given I create the following api resources
	| Name                 | DisplayName            | Secret  | Scopes           | UserClaims               |
	| estateManagement | Estate Managememt REST | Secret1 | estateManagement | MerchantId,EstateId,role |

	Given I create the following identity resources
	| Name        | DisplayName          | Description                                                 | UserClaims                                                             |
	| openid  | Your user identifier |                                                             | sub                                                                    |
	| profile | User profile         | Your user profile information (first name, last name, etc.) | name,role,email,given_name,middle_name,family_name,EstateId,MerchantId |
	| email   | Email                | Email and Email Verified Flags                              | email_verified,email                                                   |

	Given I create the following clients
	| ClientId       | Name            | Secret  | Scopes                                | GrantTypes | RedirectUris                     | PostLogoutRedirectUris            | RequireConsent | AllowOfflineAccess | ClientUri            |
	| estateUIClient | Merchant Client | Secret1 | estateManagement,openid,email,profile | hybrid     | https://[url]:[port]/signin-oidc | https://[url]:[port]/signout-oidc | false          | true               | https://[url]:[port] |


@PRTest
Scenario: Change Passwword
	Given I create the following users
	| Email Address                | Phone Number | Given Name | Middle Name | Family Name | Claims     | Roles  |
	| estateuser@testestate1.co.uk | 123456789    | Test       |             | User 1      | EstateId:1 | Estate |
	Then I get an email with a confirm email address link
	When I navigate to the confirm email address
	Then I am presented with the confirm email address successful screen
	And I get a welcome email with my login details
	Given I am on the application home page
	When I click the 'Privacy' link
	Then I am presented with a login screen
	When I login with the username 'estateuser@testestate1.co.uk' and the provided password
	Then I am presented with the privacy screen
	When I click the 'ChangePassword' link
	Then I am presented with a change password screen
	When I enter my old password '123456'
	When I enter my new password 'Pa55word!'
	And I confirm my new password 'Pa55word!'
	And I click the change password button
	Then I am returned to the application home page