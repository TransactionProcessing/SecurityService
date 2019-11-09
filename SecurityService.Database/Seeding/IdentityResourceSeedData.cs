namespace SecurityService.Database.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.EntityFramework.Entities;

    /// <summary>
    /// 
    /// </summary>
    public class IdentityResourceSeedData
    {
        /// <summary>
        /// The scope to claims mapping
        /// </summary>
        private static readonly Dictionary<String, IEnumerable<IdentityClaim>> ScopeToClaimsMapping =
            new Dictionary<String, IEnumerable<IdentityClaim>>
            {
                {
                    IdentityServerConstants.StandardScopes.Profile, new List<IdentityClaim>()
                                                                    {
                                                                        new IdentityClaim() {Type = JwtClaimTypes.Name},
                                                                        new IdentityClaim() {Type = JwtClaimTypes.Role},
                                                                        new IdentityClaim() {Type = JwtClaimTypes.Email},
                                                                        new IdentityClaim() {Type = JwtClaimTypes.GivenName},
                                                                        new IdentityClaim() {Type = JwtClaimTypes.MiddleName},
                                                                        new IdentityClaim() {Type = JwtClaimTypes.FamilyName},
                                                                    }
                },
                {
                    IdentityServerConstants.StandardScopes.OpenId, new List<IdentityClaim>()
                                                                   {
                                                                       new IdentityClaim()
                                                                       {
                                                                           Type = JwtClaimTypes.Subject
                                                                       }
                                                                   }
                },
                {
                    IdentityServerConstants.StandardScopes.Email, new List<IdentityClaim>()
                                                                  {
                                                                      new IdentityClaim() {Type = JwtClaimTypes.Email},
                                                                      new IdentityClaim() {Type = JwtClaimTypes.EmailVerified},
                                                                  }
                }
            };

        /// <summary>
        /// Gets the identity resources.
        /// </summary>
        /// <param name="seedingType">Type of the seeding.</param>
        /// <returns></returns>
        public static List<IdentityResource> GetIdentityResources(SeedingType seedingType)
        {
            return new List<IdentityResource>
                   {
                       new IdentityResource()
                       {
                           Name = IdentityServerConstants.StandardScopes.OpenId,
                           DisplayName = "Your user identifier",
                           Required = true,
                           UserClaims = IdentityResourceSeedData.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.OpenId].Select(x => new IdentityClaim
                                                                                                                                                 {
                                                                                                                                                     Type = x.Type
                                                                                                                                                 }).ToList()
                       },

                       new IdentityResource()
                       {
                           Name = IdentityServerConstants.StandardScopes.Profile,
                           DisplayName = "User profile",
                           Description = "Your user profile information (first name, last name, etc.)",
                           Emphasize = true,
                           UserClaims = IdentityResourceSeedData.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Profile].Select(x => new IdentityClaim
                                                                                                                                                  {
                                                                                                                                                      Type = x.Type
                                                                                                                                                  }).ToList()
                       },
                       new IdentityResource()
                       {
                           Name = IdentityServerConstants.StandardScopes.Email,
                           DisplayName = "Email",
                           Description = "Email and Email Verified Flags",
                           Emphasize = true,
                           UserClaims = IdentityResourceSeedData.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Email].Select(x => new IdentityClaim
                                                                                                                                                {
                                                                                                                                                    Type = x.Type
                                                                                                                                                }).ToList()
                       }
                   };
        }
    }
}