@base @shared @userlogin @forgotpassword
Feature: ForgotPassword

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

	Given I create the following users
	| Email Address                    | Password | Phone Number | Given Name | Middle Name | Family Name | Claims     | Roles      |
	| estateuser@testestate1.co.uk | 123456   | 123456789    | Test       |             | User 1      | EstateId:1 | Estate |

#@PRTest
Scenario: Forgot Password
	Given I am on the application home page
	When I click the 'Privacy' link
	Then I am presented with a login screen
	When I click on the Forgot Password Button
	Then I am presented with the forgot password screen
	When I enter my username 'estateuser@testestate1.co.uk'
	And I enter my email address 'estateuser@testestate1.co.uk'
	And I click on the Reset Password button
	Then I get an email with a forgot password link
	When I navigate to the forgot password link
	Then I am presented with the reset password screen
	When I enter my new password 'Pa55word!'
	And I confirm my new password 'Pa55word!'
	And I click the reset password button
	Then my password is reset successfully