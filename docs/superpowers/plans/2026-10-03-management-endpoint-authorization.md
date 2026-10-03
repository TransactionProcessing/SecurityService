# Management Endpoint Authorization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Require authentication for management endpoints while preserving a safe, repeatable first-install bootstrap path when the database contains no clients or users.

**Architecture:** Use one `ManagementApi` policy requiring an authenticated caller, and attach it to the management endpoint group in `ManagementEndpoints`. Defer fine-grained permission scopes/claims until a later change. Add an idempotent bootstrap service invoked by `DatabaseInitializer` that creates a configured management client and optional initial administrator user/role from deployment secrets; normal management requests remain protected from the first request onward.

**Tech Stack:** .NET 10, ASP.NET Core Minimal APIs, OpenIddict 7.7, ASP.NET Core Identity, Entity Framework Core, xUnit/NUnit/Reqnroll integration-test conventions already present in the repository.

**Spec:** GitHub issue [TransactionProcessing/SecurityService#1713](https://github.com/TransactionProcessing/SecurityService/issues/1713)

## Global Constraints

- Unauthenticated management requests must return `401`.
- Fine-grained permission separation is explicitly deferred; any valid authenticated caller may use the management endpoints in this iteration.
- First installation must not require an already-existing client or user to create the first administrative credential.
- Bootstrap must be idempotent and must not grant administrative access to an arbitrary unauthenticated request.
- Bootstrap secrets must be supplied through deployment configuration/secret storage, not committed to source-controlled `appsettings.json`.
- Existing OIDC endpoints, Razor Pages, health endpoints, and developer endpoints must not accidentally inherit management policies.

## Review Focus

- Empty database with bootstrap disabled or incompletely configured: startup must fail clearly or remain operable without exposing an anonymous admin path; cover in bootstrap tests.
- Existing database with the bootstrap client already present: startup must not duplicate clients, users, roles, or OpenIddict records; cover in idempotency tests.
- Cookie-authenticated browser requests to API endpoints: API authorization must challenge with `401`, not silently redirect to `/Account/Login`; cover in endpoint integration tests.
- Valid tokens from different authenticated client types: all management endpoints must be available in this iteration; cover with representative requests.
- Test and deployment clients that currently create management data before obtaining a token: update fixtures to provision/use the bootstrap client first; cover in integration setup tests.

---

### Task 1: Define the authenticated management policy

**Files:**
- Create: `SecurityService/Authorization/ManagementAuthorizationPolicies.cs`
- Modify: `SecurityService/Program.cs:216`
- Test: `SecurityService.UnitTests/Authorization/ManagementAuthorizationPolicyTests.cs`

**Interfaces:**
- Produces one policy named `ManagementApi`.

- [ ] **Step 1: Write policy tests** asserting `ManagementApi` allows an authenticated principal and rejects an anonymous principal.
- [ ] **Step 2: Run the focused unit tests** and verify they fail because the policy definitions do not exist.
- [ ] **Step 3: Register `ManagementApi` through `AddAuthorization` using only `RequireAuthenticatedUser()`; do not add permission claims, permission scopes, or role requirements.
- [ ] **Step 4: Configure API challenge behavior** so an unauthenticated request to a management endpoint produces `401` rather than a cookie login redirect. Preserve normal browser login behavior for Razor Pages.
- [ ] **Step 5: Run the focused unit tests** and verify they pass.
- [ ] **Step 6: Commit** with message `feat: require authentication for management API`.

### Task 2: Apply policies to management endpoint groups

**Files:**
- Modify: `SecurityService/Endpoints/ManagementEndpoints.cs:5`
- Test: `SecurityService.UnitTests/Endpoints/ManagementEndpointsTests.cs` or the repository’s existing endpoint-test location

**Interfaces:**
- `MapManagementEndpoints` must expose one policy-bearing management route group covering all existing `/api/clients`, `/api/apiscopes`, `/api/apiresources`, `/api/identityresources`, `/api/roles`, and `/api/users` routes.

- [ ] **Step 1: Add endpoint metadata tests** verifying every management route has the `ManagementApi` policy and that no management route remains unprotected.
- [ ] **Step 2: Run the focused endpoint tests** and verify they fail for the current unannotated routes.
- [ ] **Step 3: Refactor endpoint mapping into one policy-bearing `MapGroup`** while preserving route templates, handler methods, names, and response behavior.
- [ ] **Step 4: Run the focused endpoint tests** and verify they pass.
- [ ] **Step 5: Commit** with message `feat: protect management endpoints with policies`.

### Task 3: Add bootstrap-client configuration and first-install provisioning

**Files:**
- Modify: `SecurityService.BusinessLogic/SecurityServiceOptions.cs`
- Modify: `SecurityService/HostedServices/DatabaseInitializer.cs:11`
- Modify: `SecurityService/appsettings.json` only for non-secret defaults/documentation-safe structure; do not add a usable secret
- Modify: `SecurityService/appsettings.development.json` only if local development needs an explicitly opt-in bootstrap configuration
- Create: `SecurityService/HostedServices/ManagementBootstrapper.cs`
- Create: `SecurityService.UnitTests/HostedServices/ManagementBootstrapperTests.cs`

**Interfaces:**
- Add an options section such as `ServiceOptions:ManagementBootstrap` with:
  - `Enabled` (`bool`, default `false`)
  - `ClientId` (`string`)
  - `ClientSecret` (`string`)
  - `ClientName` (`string`)
  - `AdminUserName` (`string?`)
  - `AdminEmail` (`string?`)
  - `AdminPassword` (`string?`)
- Add `ManagementBootstrapper.InitializeAsync(CancellationToken)` returning a result/diagnostic object or throwing a descriptive startup exception on invalid enabled configuration.
- Do not add management permission scopes in this iteration; the bootstrap client only needs to be a valid authenticated client.

- [ ] **Step 1: Write bootstrap tests** for disabled bootstrap, missing required configuration, first-run client creation, rerun idempotency, and existing partial records.
- [ ] **Step 2: Run the focused bootstrap tests** and verify they fail because the bootstrap service/options do not exist.
- [ ] **Step 3: Add configuration binding and validation** with bootstrap disabled by default; when enabled, require client ID and secret, and require a complete admin-user tuple if admin-user creation is requested.
- [ ] **Step 4: Implement idempotent bootstrap-client creation** as an OpenIddict application with `client_credentials` support. Keep the secret out of logs and do not overwrite an existing client secret silently.
- [ ] **Step 5: Implement optional initial-admin creation** only if the existing application requires a seeded human user; do not attach management permission claims in this iteration. Fail safely if a conflicting record exists with incompatible configuration.
- [ ] **Step 6: Invoke the bootstrapper from `DatabaseInitializer` after migrations**, ensuring startup ordering is migrations → bootstrap records.
- [ ] **Step 7: Run the focused bootstrap tests** and verify they pass, including a second initialization against the same database.
- [ ] **Step 8: Commit** with message `feat: add first-install management bootstrap`.

### Task 4: Verify authenticated token flow

**Files:**
- Modify: the OpenIddict server/token configuration in `SecurityService/Program.cs`
- Modify: the relevant token/request handler or claims configuration discovered during implementation
- Test: `SecurityService.UnitTests/Oidc/OidcEndpointTests.cs` or a new focused token authorization test

**Interfaces:**
- A token issued to the configured bootstrap client authenticates successfully at every management endpoint.
- Any valid access token accepted by the existing OpenIddict validation configuration satisfies `ManagementApi`.

- [ ] **Step 1: Write token tests** for an allowed bootstrap client and an invalid/absent token.
- [ ] **Step 2: Run the focused token tests** and verify they fail until the policy and bootstrap client are wired.
- [ ] **Step 3: Verify the existing OpenIddict client-credentials flow** issues a token for the bootstrapped client and that the validation principal is authenticated.
- [ ] **Step 4: Run the focused token tests** and verify they pass.
- [ ] **Step 5: Commit** with message `test: verify authenticated management token flow`.

### Task 5: Add end-to-end authorization coverage

**Files:**
- Create or modify: the existing `SecurityService.IntegrationTests` authorization-test feature/steps
- Modify: `SecurityService.IntegrationTesting.Helpers/SecurityServiceSteps.cs` and test HTTP-client setup as needed
- Create or modify: integration-test application factory/bootstrap configuration

**Interfaces:**
- Tests must be able to obtain a valid access token and send it as `Authorization: Bearer <token>`.
- Tests must be able to issue requests without credentials.

- [ ] **Step 1: Add scenarios** covering `401` for unauthenticated requests and `2xx` for allowed access on representative management endpoints.
- [ ] **Step 2: Add a clean-database scenario** that starts with no clients/users, runs bootstrap, obtains a client-credentials token, and successfully performs an initial protected management operation.
- [ ] **Step 3: Update existing integration setup** so it no longer assumes management creation endpoints are anonymous; provision the test management client through the bootstrap configuration before creating fixtures.
- [ ] **Step 4: Run the relevant integration-test projects** and verify all authorization and existing management scenarios pass.
- [ ] **Step 5: Commit** with message `test: cover authenticated management access and bootstrap`.

### Task 6: Document deployment and operational rollout

**Files:**
- Create or modify: `docs/management-authorization.md`
- Modify: deployment/sample configuration documentation used by the repository

- [ ] **Step 1: Document first installation**: enable bootstrap, supply the client secret through secret storage, start the service, obtain an access token, create normal clients/users, then disable or rotate bootstrap credentials.
- [ ] **Step 2: Document upgrade behavior** for existing installations: deploy the policy changes, provision a management client out-of-band or enable bootstrap temporarily, then verify scoped access before disabling bootstrap.
- [ ] **Step 3: Document that this iteration provides authentication-only protection, with fine-grained permission authorization deferred; document expected `401` responses and successful authenticated access.
- [ ] **Step 4: Run the complete solution test suite** and record the exact command/result in the implementation PR.
- [ ] **Step 5: Commit** with message `docs: document management authorization bootstrap`.

## Final Verification

- [ ] Run focused unit tests for authorization policies, endpoint metadata, bootstrap, and tokens.
- [ ] Run management integration tests, including the clean-database bootstrap scenario.
- [ ] Run the complete solution build and test commands used by CI.
- [ ] Inspect the final diff for secrets, unintended policy inheritance, changed OIDC/Razor behavior, and duplicate bootstrap records.
- [ ] Record that the issue’s fine-grained permission-separation criterion is intentionally deferred to a follow-up issue.
