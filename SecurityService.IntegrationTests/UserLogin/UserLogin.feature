@base @roles @apiresources @clients @users @userlogin
Feature: UserLogin

Background: 

	Given I create the following roles
	| Role Name |
	| Estate    |

	Given I create the following api resources
	| Name                    | DisplayName                    | Secret  | Scopes                  | UserClaims                 |
	| estateManagement        | Estate Managememt REST         | Secret1 | estateManagement        | MerchantId, EstateId, role |

	Given I create the following clients
	| ClientId       | Name            | Secret  | Scopes           | GrantTypes | RedirectUris                        | PostLogoutRedirectUris               | RequireConsent |
	| estateUIClient | Merchant Client | Secret1 | estateManagement | password   | http://localhost:[port]/signin-oidc | http://localhost:[port]/signout-oidc | false          |

	Given I create the following users
	| Email Address                | Password | Phone Number | Given Name | Middle Name | Family Name | Claims     | Roles  |
	| estateuser@testestate1.co.uk | 123456   | 123456789    | Test       |             | User 1      | EstateId:1 | Estate |

@PRTest
Scenario: Access Secure Area In Application
	Given I am on the application home page
	When I click the 'Privacy' link
	Then I am presented with a login screen
	#When I login with the username 'estateuser@testestate1.co.uk' and password '123456'
	#Then I am presented with the privacy screen
