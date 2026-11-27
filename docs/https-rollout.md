# SecurityService HTTPS deployment

SecurityService listens on HTTPS port 5001. Kestrel loads its server certificate through `TransactionProcessing.CertificateDefaults` from deployment configuration. Provide the password-protected PFX outside the repository and configure:

- `ServiceOptions:KestrelOptions:Path`: absolute path to the issued PFX on the service host.
- `ServiceOptions:KestrelOptions:Password`: PFX password from the deployment secret store.

The service can receive these values as environment variables using the .NET configuration names:

```ini
ServiceOptions__KestrelOptions__Path=<path to the PFX>
ServiceOptions__KestrelOptions__Password=<PFX password>
```

The PFX must contain the leaf certificate and any required intermediate certificates. Its subject alternative names must cover the configured `ServiceOptions:PublicOrigin`, `ServiceOptions:IssuerUrl`, and `HealthMonitoring:Service:HealthUrl` hostnames or IP addresses. The service fails startup when the certificate is missing or invalid. After replacing a certificate, restart the service.

The staging and production release jobs read the certificate path and password from the GitHub environment secrets `SECURITYSERVICE_CERTIFICATE_PATH` and `SECURITYSERVICE_CERTIFICATE_PASSWORD`, then set them as service-specific environment variables. The database connection string from `AUTHENTICATION_DB_CONNECTION_STRING` is stored in a deployment-only `appsettings.local.json` beside the executable. That file is excluded from source control and restricted to SYSTEM and local administrators. Supply all three GitHub environment secrets in both deployment environments. Before deploying, install the CA root in the Windows trust store used by the service and any callers, and place the issued PFX at the configured path on that host.

For local development, set `ServiceOptions__KestrelOptions__Path` and `ServiceOptions__KestrelOptions__Password` in the process environment, or use user secrets. Configure `ConnectionStrings:AuthenticationDbContext` through user secrets or a local `appsettings.local.json`. Never add certificates, passwords, or private keys to source control. Integration tests generate a temporary CA and server certificate for their containers.
