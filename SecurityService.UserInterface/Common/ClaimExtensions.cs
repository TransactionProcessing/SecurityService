using Duende.IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.UserInterface.Common
{
    public static class ClaimExtensions
    {
        public static string? GetClaimValue(this IEnumerable<Claim> claims, params string[] types)
        {
            return types
                .Select(t => claims.FirstOrDefault(c => c.Type == t)?.Value)
                .FirstOrDefault(v => v != null);
        }

        public static List<Claim> BuildDisplayNameClaims(this IEnumerable<Claim> claims)
        {
            // 1. Name
            var name = claims.GetClaimValue(JwtClaimTypes.Name, ClaimTypes.Name);
            if (name != null)
            {
                return new List<Claim> { new Claim(JwtClaimTypes.Name, name) };
            }

            // 2. Given/Family name fallback
            var first = claims.GetClaimValue(JwtClaimTypes.GivenName, ClaimTypes.GivenName);
            var last = claims.GetClaimValue(JwtClaimTypes.FamilyName, ClaimTypes.Surname);

            var full = (first, last) switch
            {
                (not null, not null) => $"{first} {last}",
                (not null, null) => first,
                (null, not null) => last,
                _ => null
            };

            return full != null
                ? new List<Claim> { new Claim(JwtClaimTypes.Name, full) }
                : new List<Claim>();
        }
    }
}
