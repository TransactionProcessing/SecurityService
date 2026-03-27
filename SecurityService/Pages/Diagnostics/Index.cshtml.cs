using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecurityService.Pages.Diagnostics;

public sealed class IndexModel : PageModel
{
    public IReadOnlyCollection<DiagnosticItem> Claims { get; private set; } = Array.Empty<DiagnosticItem>();

    public IReadOnlyCollection<DiagnosticItem> Properties { get; private set; } = Array.Empty<DiagnosticItem>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (this.Request.HttpContext.Connection.RemoteIpAddress is { } remoteIpAddress &&
            IPAddress.IsLoopback(remoteIpAddress) == false)
        {
            return this.NotFound();
        }

        var result = await this.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result.Succeeded == false || result.Principal is null)
        {
            return this.Challenge(IdentityConstants.ApplicationScheme);
        }

        this.Claims = result.Principal.Claims
            .Select(claim => new DiagnosticItem(claim.Type, claim.Value))
            .OrderBy(item => item.Type, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        this.Properties = result.Properties?.Items
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(item => new DiagnosticItem(item.Key, item.Value ?? string.Empty))
            .ToArray() ?? Array.Empty<DiagnosticItem>();

        return this.Page();
    }
}

public sealed record DiagnosticItem(string Type, string Value);
