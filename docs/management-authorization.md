# Management API authentication

All management endpoints under `/api` now require an authenticated access token. This iteration intentionally does not distinguish permissions between users, clients, resources, or roles. Fine-grained authorization can be added later without changing the bootstrap mechanism.

Unauthenticated API requests return `401`. Browser requests to Razor Pages continue to use the normal login redirect flow.

## First installation

The service can create a client-credentials client during database initialization. Enable it only through deployment configuration or a secret store:

```text
ServiceOptions:ManagementBootstrap:Enabled=true
ServiceOptions:ManagementBootstrap:ClientId=management-bootstrap
ServiceOptions:ManagementBootstrap:ClientSecret=<secret-from-secret-store>
```

On startup, the initializer:

1. Applies database migrations.
2. Creates the configured OpenIddict application if it does not exist.
3. Creates the matching `ClientDefinition` if it does not exist.
4. Leaves existing records unchanged on later startups.

Use the bootstrap client with `client_credentials` at `/connect/token`, then send the returned access token as:

```http
Authorization: Bearer <access-token>
```

After normal clients/users have been provisioned, disable or rotate the bootstrap credential. Do not place the secret in source-controlled `appsettings.json` or logs.

An optional initial Identity administrator can also be configured with the complete `AdminUserName`, `AdminEmail`, and `AdminPassword` option set. The password must be supplied through secret storage.

## Existing installations

Existing deployments must provision a valid management client before enabling the protected endpoints. The bootstrap settings can be enabled temporarily for the upgrade, or the client can be created through an out-of-band provisioning process.
