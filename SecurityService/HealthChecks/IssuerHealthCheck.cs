using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SecurityService.BusinessLogic;

namespace SecurityService.HealthChecks;

public sealed class IssuerHealthCheck : IHealthCheck
{
    private readonly ServiceOptions _options;

    public IssuerHealthCheck(IOptions<ServiceOptions> options)
    {
        this._options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (Uri.TryCreate(this._options.IssuerUrl, UriKind.Absolute, out var issuer) == false)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("The configured issuer is not a valid absolute URI."));
        }

        return Task.FromResult(issuer.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            ? HealthCheckResult.Healthy("The issuer is configured for HTTPS.")
            : HealthCheckResult.Degraded("The issuer is configured without HTTPS."));
    }
}
