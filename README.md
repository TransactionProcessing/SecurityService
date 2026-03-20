# SecurityService

SecurityService exposes a stable REST management API for identity administration:

- clients
- users
- roles
- API resources
- API scopes
- identity resources

The service currently stores and manages those objects by using Duende IdentityServer and ASP.NET Identity internally.

## IdentityServer to Keycloak migration

If you want to migrate from IdentityServer to Keycloak, the safest split is:

- use **Keycloak directly** for OAuth2 / OpenID Connect runtime traffic such as discovery, authorization, token, user-info and logout endpoints
- keep **SecurityService** as the management API façade for your existing administrative workflows

That means your existing `/api/...` endpoints can remain the contract used by your operational tooling, while the underlying implementation is changed to talk to the Keycloak Admin API instead of the IdentityServer stores.

### What can stay the same

The current management routes are already separate from the OIDC endpoints:

- `POST /api/clients`
- `GET /api/clients`
- `GET /api/clients/{clientId}`
- `POST /api/users`
- `GET /api/users`
- `GET /api/users/{userId}`
- `POST /api/roles`
- `GET /api/roles`
- `GET /api/roles/{roleId}`
- `POST /api/apiresources`
- `GET /api/apiresources`
- `GET /api/apiresources/{name}`
- `POST /api/apiscopes`
- `GET /api/apiscopes`
- `GET /api/apiscopes/{name}`
- `POST /api/identityresources`
- `GET /api/identityresources`
- `GET /api/identityresources/{name}`

These endpoints are the right layer to preserve when you want SecurityService to act as a wrapper over Keycloak.

### What changes underneath

The current implementation is still IdentityServer-specific in the business logic layer. A Keycloak migration therefore means replacing the storage and admin operations behind the existing API surface:

- clients map to Keycloak clients
- users map to Keycloak users
- roles map to Keycloak realm or client roles
- API scopes / identity resources usually map to Keycloak client scopes and protocol mapper configuration

### Recommended migration approach

1. Keep the REST management API contract unchanged for callers.
2. Move all OIDC and token consumers to Keycloak endpoints directly.
3. Replace the internal management implementation so that `/api/clients`, `/api/users`, `/api/roles`, and the resource/scope endpoints call the Keycloak Admin API.
4. Treat Keycloak as the source of truth for clients, users and claims during the migration.

In short: **yes, the API can be maintained as the wrapper layer**.

This repository now separates the request handlers from the current IdentityServer/ASP.NET Identity implementation by routing administrative operations through an internal identity management abstraction. The default implementation is still IdentityServer-backed, but a future Keycloak-backed implementation can be added behind the same abstraction without changing the public `/api/...` endpoints.

## Keycloak issuer URL overrides in integration environments

Yes — Keycloak can be used in the same kind of integration-test setup where the URL used to **reach** the provider is different from the issuer that appears inside tokens, but you need to treat those as two separate settings:

- `Keycloak.ServerUrl`: where SecurityService can call the Keycloak admin/token endpoints
- `Keycloak.IssuerUrl`: the issuer base URL that downstream OIDC clients should accept in the `iss` claim

For the test UI / OIDC client side, the same split is supported through:

- `AppSettings:Authority`
- `AppSettings:IssuerUrl`
- `AppSettings:MetadataAddress` (optional override)
- `AppSettings:AuthorizationEndpoint` (optional override)
- `AppSettings:RequireHttpsMetadata` (set this to `false` for a local HTTP Keycloak testcontainer)

This is useful when:

- Keycloak is reachable on an internal Docker hostname for backchannel traffic
- but tokens are issued with a frontend hostname, reverse-proxy URL, or a different externally configured realm issuer

If you do not set `IssuerUrl`, the UI falls back to `Authority`, which matches the previous IdentityServer behavior.
