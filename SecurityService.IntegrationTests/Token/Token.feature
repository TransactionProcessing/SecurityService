@base @token @clients @apiresources @users @roles
Feature: Token
	
Background: 

	Given I create the following roles
	| Role Name |
	| Estate    |
	| Merchant  |

	Given I create the following api resources
	| Name                    | DisplayName                    | Secret  | Scopes                  | UserClaims                 |
	| estateManagement        | Estate Managememt REST         | Secret1 | estateManagement        | MerchantId, EstateId, role |
	| transactionProcessor    | Transaction Processor REST     | Secret1 | transactionProcessor    |                            |
	| transactionProcessorAcl | Transaction Processor ACL REST | Secret1 | transactionProcessorAcl | MerchantId, EstateId, role |

	Given I create the following clients
	| ClientId       | Name            | Secret  | Scopes                                                        | GrantTypes         |
	| serviceClient  | Service Client  | Secret1 | estateManagement,transactionProcessor,transactionProcessorAcl | client_credentials |
	| merchantClient | Merchant Client | Secret1 | transactionProcessorAcl                                       | password           |

	Given I create the following users
	| Email Address           | Password | Phone Number | Given Name | Middle Name | Family Name | Claims                  | Roles    |
	| merchantuser@testmerchant1.co.uk | 123456   | 123456789    | Test       |             | User 1      | EstateId:1,MerchantId:2 | Merchant |

@PRTest
Scenario: Get Client Token
	When I request a client token with the following values
	| ClientId      | ClientSecret |
	| serviceClient | Secret1      |
	Then my token is returned

@PRTest
Scenario: Get Password Token
	When I request a password token with the following values
	| ClientId       | ClientSecret | Username                         | Password |
	| merchantClient | Secret1      | merchantuser@testmerchant1.co.uk | 123456   |
	Then my token is returned


