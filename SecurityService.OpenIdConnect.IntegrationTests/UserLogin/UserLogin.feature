@base @shared @userlogin
Feature: User Login

Background: 

	Given I create the following roles
	| Role Name  |
	| Estate |

	Given I create the following api resources
	| Name                 | DisplayName                | Secret  | Scopes               | UserClaims               |
	| transactionProcessor | Transaction Processor REST | Secret1 | transactionProcessor | MerchantId,EstateId,role |

	Given I create the following identity resources
	| Name    | DisplayName          | Description                                                 | UserClaims                                                             |
	| openid  | Your user identifier |                                                             | sub                                                                    |
	| profile | User profile         | Your user profile information (first name, last name, etc.) | name,role,email,given_name,middle_name,family_name,EstateId,MerchantId |
	| email   | Email                | Email and Email Verified Flags                              | email_verified,email                                                   |

	Given I create the following clients
	| ClientId       | Name            | Secret  | Scopes                                    | GrantTypes | RedirectUris                     | PostLogoutRedirectUris            | RequireConsent | AllowOfflineAccess | ClientUri            |
	| estateUIClient | Merchant Client | Secret1 | transactionProcessor,openid,email,profile,offline_access | authorization_code | https://[url]:[port]/signin-oidc | https://[url]:[port]/signout-oidc | false          | true               | https://[url]:[port] |

@PRTest
Scenario: Create User and Login
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

@mfa
Scenario: Enroll MFA and complete an MFA login
	Given I create the following users
	| Email Address                | Phone Number | Given Name | Middle Name | Family Name | Claims     | Roles  |
	| mfauser@testestate1.co.uk    | 123456789    | Test       |             | User 1      | EstateId:1 | Estate |
	Then I get an email with a confirm email address link
	When I navigate to the confirm email address
	Then I am presented with the confirm email address successful screen
	And I get a welcome email with my login details
	Given I am on the application home page
	When I click the 'Privacy' link
	Then I am presented with a login screen
	When I login with the username 'mfauser@testestate1.co.uk' and the provided password
	Then I am presented with the privacy screen
	When I open the hosted MFA management page
	And I begin MFA enrollment
	Then I am shown the MFA authenticator setup key
	When I confirm MFA enrollment with the current authenticator code
	Then MFA is enabled
	When I sign out of SecurityService
	Given I am on the application home page
	When I click the 'Privacy' link
	Then I am presented with a login screen
	When I login with the username 'mfauser@testestate1.co.uk' and the provided password
	Then I am presented with the MFA verification screen
	When I complete MFA with the current authenticator code
	Then I am presented with the privacy screen
	

