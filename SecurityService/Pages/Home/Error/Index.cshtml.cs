using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecurityService.Pages.Home.Error;

public sealed class IndexModel : PageModel
{
    public string? Error { get; private set; }
    public string? ErrorDescription { get; private set; }

    public string? OriginalPath { get; private set; }

    public string? OriginalQueryString { get; private set; }

    public int? ResponseStatusCode { get; private set; }

    public void OnGet()
    {
        var exceptionFeature = this.HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature is not null)
        {
            this.Error = $"{exceptionFeature.Error.GetType().Name}: {exceptionFeature.Error.Message}";
            this.ErrorDescription = "An unhandled exception occurred while processing the request.";
            this.OriginalPath = exceptionFeature.Path;
            this.ResponseStatusCode = 500;
            return;
        }

        var statusCodeFeature = this.HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (statusCodeFeature is not null)
        {
            this.OriginalPath = statusCodeFeature.OriginalPathBase + statusCodeFeature.OriginalPath;
            this.OriginalQueryString = statusCodeFeature.OriginalQueryString;
            this.ResponseStatusCode = this.HttpContext.Response.StatusCode;
        }

        var response = this.HttpContext.GetOpenIddictServerResponse();
        this.Error = response?.Error ?? (this.ResponseStatusCode.HasValue ? $"Request failed with status code {this.ResponseStatusCode.Value}." : "An unexpected error occurred.");
        this.ErrorDescription = response?.ErrorDescription ?? (this.OriginalPath is null ? null : $"Original request: {this.OriginalPath}{this.OriginalQueryString}");
    }
}
