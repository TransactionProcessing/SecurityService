# OAuth Grant Restrictions and PKCE Design

## Goal

Restrict SecurityService OAuth/OIDC capabilities to approved flows while preserving an explicit, client-scoped compatibility path for legacy consumers during migration.

The approved default flows are:

- Authorization code with PKCE
- Client credentials
- Refresh token

Device authorization is also excluded because no current supported client requires it.

Implicit, hybrid, and resource-owner-password flows must not be available by default.

## Current state

SecurityService globally enables authorization-code, client-credentials, device, hybrid, implicit, password, and refresh-token flows. It also ignores OpenIddict grant-type permissions. Client registrations validate and persist `AllowedGrantTypes`, but those values are not translated into enforceable OpenIddict application permissions.

The OIDC token handler has an explicit password-grant branch. Authorization-code, refresh-token, and device-code requests share the code/refresh handling path. Existing clients and downstream applications therefore depend on both global server capabilities and the custom client grant-type list.

## Design

### Server capabilities

`Program.cs` will always enable the three approved flows. Legacy flow capabilities will only be enabled when the corresponding legacy exception configuration is present. The server will no longer call `IgnoreGrantTypePermissions()`.

Authorization-code clients will receive the OpenIddict proof-key-for-code-exchange requirement. The authorization endpoint will therefore reject authorization-code requests that do not include a valid PKCE challenge and the token exchange will require the matching verifier.

### Client permissions

Client creation will map each requested `AllowedGrantTypes` value to OpenIddict application permissions:

- `authorization_code` → authorization-code grant and code response permissions
- `client_credentials` → client-credentials grant permission
- `refresh_token` → refresh-token grant permission
- legacy `password` → password grant permission only when configured for that client
- legacy `hybrid` → hybrid response permissions only when configured for that client

The existing `AllowedGrantTypesJson` storage and API contract will remain in place. No database schema migration is expected. New and updated OpenIddict application records must receive permissions at creation time; existing records will require an explicit reconciliation path or controlled re-registration before enforcement is enabled.

### Legacy exceptions

Legacy exceptions will be configuration-driven and client-scoped. The configuration will contain a map of legacy flow names to approved client IDs, defaulting to empty. A client may request a legacy flow only when its client ID appears in the matching map. Configuration enabling a legacy flow will also enable that server capability; with no entries, the flow remains disabled.

This keeps legacy support explicit and reviewable without making legacy grants part of the normal client-registration allowlist.

### Token handling

The existing password-grant handler remains only as a compatibility path. It will be unreachable unless both server legacy configuration and client permissions allow it. Unsupported or unauthorized grant requests will continue to return the OAuth `unsupported_grant_type`/permission error response produced by OpenIddict or the existing token handler.

The normal authorization-code, client-credentials, and refresh-token paths remain unchanged apart from permission and PKCE enforcement. Device authorization endpoints and handling will be removed or disabled because no supported client requires them.

## Compatibility and rollout

The secure default is intentionally breaking for clients currently using password or hybrid flows. During rollout, deployments that still need those clients can add explicit client IDs to the legacy exception configuration. The exception should be removed after the mobile and browser clients migrate.

The management bootstrap client remains client-credentials-only and does not need a special exception.

## Testing strategy

Unit tests will cover:

- approved grant types accepted by client registration;
- implicit, password, and hybrid grants rejected when no exception is configured;
- configured legacy grants accepted only for the named client IDs;
- OpenIddict permissions/requirements generated for an authorization-code + PKCE client;
- existing client lifecycle behavior remaining intact.

Integration tests will cover:

- approved client-credentials token issuance;
- unsupported grant types rejected by `/connect/token`;
- password and hybrid requests rejected by default;
- an explicitly configured legacy client succeeding;
- authorization-code requests requiring PKCE;
- refresh-token behavior remaining functional.

Existing integration fixtures that use `hybrid` or `password` will either be converted to approved flows or marked as explicit legacy-exception scenarios.

## Out of scope

- Migrating EstateManagementUI or TransactionMobile clients.
- Supporting device authorization for future clients.
- Redesigning the SecurityService login page.
- Changing token claims, scopes, lifetimes, or API authorization semantics.
- Removing the password handler immediately; it remains available only behind the controlled compatibility path.
