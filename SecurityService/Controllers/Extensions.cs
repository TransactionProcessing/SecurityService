namespace SecurityService.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Duende.IdentityServer.Models;
    using IdentityServerHost.Quickstart.UI;
    using Microsoft.AspNetCore.Mvc;
    
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        /// <summary>
        /// Checks if the redirect URI is for a native client.
        /// </summary>
        /// <returns></returns>
        public static bool IsNativeClient(this AuthorizationRequest context)
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
               && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }
    }
}
