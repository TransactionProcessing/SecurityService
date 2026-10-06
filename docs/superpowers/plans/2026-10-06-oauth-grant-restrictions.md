# OAuth Grant Restrictions and PKCE Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restrict SecurityService to authorization code + PKCE, client credentials, and refresh tokens by default, while supporting explicitly configured client-scoped legacy exceptions.

**Architecture:** Centralize grant/permission translation in a BusinessLogic helper used by client registration and startup reconciliation. Bind legacy exception configuration through `ServiceOptions`, enable legacy server capabilities only when configured, and let OpenIddict enforce application permissions and PKCE requirements. Preserve the existing `AllowedGrantTypesJson` persistence/API contract.

**Tech Stack:** .NET 10, ASP.NET Core, OpenIddict 7.7.1, EF Core, xUnit/NUnit project tests, Reqnroll integration tests.

**Spec:** `docs/superpowers/specs/2026-10-06-oauth-grant-restrictions-design.md`

## Global Constraints

- Approved default flows are authorization code with PKCE, client credentials, and refresh token.
- Implicit, hybrid, and password flows are unavailable unless explicitly configured as legacy client exceptions. Device authorization has no supported client and remains disabled.
- Existing `AllowedGrantTypesJson` storage and client-management DTOs remain compatible.
- Authorization-code clients must receive the proof-key-for-code-exchange requirement.
- Do not migrate EstateManagementUI or TransactionMobile in this change.
- Do not change token claims, scopes, lifetimes, or API authorization semantics.

## Review Focus

- A client requesting an approved grant receives only the matching OpenIddict permissions, and cannot use another grant.
- A legacy grant is rejected when its client ID is absent from configuration, even if the grant name is otherwise recognized.
- Existing persisted clients are reconciled before permission enforcement, rather than becoming unusable after deployment.
- Authorization-code clients without PKCE are rejected while valid PKCE exchanges continue to work.
- The management bootstrap client remains client-credentials-only and does not gain legacy permissions.

---

### Task 1: Add failing permission-mapping tests and the grant policy helper

**Files:**
- Create: `SecurityService.BusinessLogic/Oidc/OAuthGrantPolicy.cs`
- Test: `SecurityService.UnitTests/Oidc/OAuthGrantPolicyTests.cs`

**Interfaces:**
- Produces `OAuthGrantPolicy.CreatePermissions(IReadOnlyCollection<string> allowedGrantTypes, string clientId, OAuthOptions options)` returning an immutable permission/requirement result suitable for `OpenIddictApplicationDescriptor`.
- Produces `OAuthGrantPolicy.IsGrantAllowed(string grantType, string clientId, OAuthOptions options)` for registration and reconciliation validation.

- [ ] **Step 1: Write failing tests** for authorization-code + refresh, client credentials, empty/unknown grants, password/hybrid/implicit without exceptions, and configured legacy client IDs.
- [ ] **Step 2: Run the focused tests and verify they fail** because the policy helper does not exist.

  Run: `dotnet test SecurityService.UnitTests/SecurityService.UnitTests.csproj --filter FullyQualifiedName~OAuthGrantPolicyTests`

- [ ] **Step 3: Implement the minimal policy helper** using OpenIddict grant, endpoint, response-type, and proof-key permission constants. Map authorization-code clients to the code response type plus PKCE requirement; map hybrid to the required authorization-code and code/id-token response permissions only as a legacy exception.
- [ ] **Step 4: Run the focused tests and verify they pass.**
- [ ] **Step 5: Commit** with `git commit -m "Add OAuth grant policy mapping"`.

### Task 2: Add OAuth options and registration validation

**Files:**
- Modify: `SecurityService.BusinessLogic/SecurityServiceOptions.cs`
- Modify: `SecurityService/Program.cs`
- Modify: `SecurityService.BusinessLogic/RequestHandlers/ClientRequestHandler.cs`
- Modify: `SecurityService.UnitTests/RequestHandlers/ClientRequestHandlerTests.cs`

**Interfaces:**
- Add `ServiceOptions.OAuth` with `OAuthOptions.LegacyGrantTypeClients`, a configuration-bindable map from legacy grant name to client IDs.
- Inject `IOptions<ServiceOptions>` into `ClientRequestHandler` and use `OAuthGrantPolicy` for validation and descriptor permissions.

- [ ] **Step 1: Add failing client-handler tests** proving password, hybrid, and implicit registrations are invalid by default and valid only for configured client IDs; assert authorization-code registrations receive PKCE permissions/requirements through the persisted OpenIddict application.
- [ ] **Step 2: Run the focused tests and verify the new cases fail.**
- [ ] **Step 3: Add `OAuthOptions` defaults and bind `ServiceOptions:OAuth`** without changing existing configuration behavior.
- [ ] **Step 4: Update client creation** to reject unauthorized legacy grants, create the OpenIddict descriptor with the policy-generated permissions/requirements, and retain the existing custom JSON grant list.
- [ ] **Step 5: Run `ClientRequestHandlerTests` and verify all pass.**
- [ ] **Step 6: Commit** with `git commit -m "Enforce OAuth grants during client registration"`.

### Task 3: Configure the server’s default and legacy capabilities

**Files:**
- Modify: `SecurityService/Program.cs`
- Modify: `SecurityService/appsettings.json`
- Modify: `SecurityService/appsettings.development.json` if present
- Modify: `SecurityService/appsettings.staging.json` if present
- Test: `SecurityService.UnitTests/Configuration/OAuthOptionsTests.cs`

- [ ] **Step 1: Write failing configuration tests** for empty legacy configuration and for each configured legacy flow enabling only the corresponding capability.
- [ ] **Step 2: Run the focused configuration tests and verify they fail.**
- [ ] **Step 3: Remove the device authorization endpoint URI and device, implicit, hybrid, and password capability calls from the unconditional server setup; retain authorization code, client credentials, and refresh token capabilities. Remove `IgnoreGrantTypePermissions()` and add conditional legacy capability calls based on `OAuthOptions.LegacyGrantTypeClients`.
- [ ] **Step 4: Add an empty `ServiceOptions:OAuth:LegacyGrantTypeClients` configuration section** to shipped configuration examples, with comments/documentation explaining that entries are deployment exceptions.
- [ ] **Step 5: Run configuration and existing unit tests.**
- [ ] **Step 6: Commit** with `git commit -m "Restrict default OAuth server capabilities"`.

### Task 4: Reconcile existing OpenIddict applications at startup

**Files:**
- Create: `SecurityService/HostedServices/OAuthApplicationSynchronizer.cs`
- Modify: `SecurityService/HostedServices/DatabaseInitializer.cs`
- Modify: `SecurityService/Program.cs` for dependency registration if needed
- Test: `SecurityService.UnitTests/HostedServices/OAuthApplicationSynchronizerTests.cs`

**Interfaces:**
- Add `OAuthApplicationSynchronizer.SynchronizeAsync(CancellationToken)` that reads `ClientDefinitions`, validates their grant lists against the current policy, and updates matching OpenIddict application descriptors with permissions and PKCE requirements.

- [ ] **Step 1: Write failing synchronizer tests** for an existing authorization-code client, an existing client-credentials client, an unauthorized legacy client causing a clear startup failure, and the management bootstrap client remaining client-credentials-only.
- [ ] **Step 2: Run the focused tests and verify they fail.**
- [ ] **Step 3: Implement synchronization** after database migration and before management bootstrap initialization; do not overwrite client secrets or redirect URIs, and fail fast on persisted clients that require an unconfigured legacy exception.
- [ ] **Step 4: Run synchronizer and management-bootstrap tests.**
- [ ] **Step 5: Commit** with `git commit -m "Reconcile OAuth application permissions at startup"`.

### Task 5: Add OIDC endpoint regression coverage

**Files:**
- Modify: `SecurityService.UnitTests/Oidc/OidcEndpointTests.cs` or create focused token tests beside it
- Modify: `SecurityService.IntegrationTests/Token/Token.feature`
- Modify: `SecurityService.IntegrationTests/Token/TokenSteps.cs` as required by the scenarios

- [ ] **Step 1: Add failing integration scenarios** for successful client credentials, password rejection by default, an unauthorized grant rejection, and successful refresh-token exchange where existing fixture support permits.
- [ ] **Step 2: Run the focused token feature/tests and verify the new scenarios fail.**
- [ ] **Step 3: Implement only the endpoint/test support needed; rely on OpenIddict permission enforcement rather than duplicating grant checks in the token handler.**
- [ ] **Step 4: Run the focused token integration suite and verify it passes.**
- [ ] **Step 5: Commit** with `git commit -m "Cover OAuth grant rejection behavior"`.

### Task 6: Update repository fixtures and documentation

**Files:**
- Modify: `SecurityService.OpenIdConnect.IntegrationTests/UserLogin/UserLogin.feature`
- Modify: `SecurityService.OpenIdConnect.IntegrationTests/ChangePassword/ChangePassword.feature`
- Modify: `SecurityService.OpenIdConnect.IntegrationTests/ForgotPassword/ForgotPassword.feature`
- Modify: `SecurityService.IntegrationTests/Clients/Clients.feature`
- Modify: `SecurityService.IntegrationTests/Token/Token.feature`
- Modify: `docs/management-authorization.md` or create `docs/oauth-grant-policy.md`

- [ ] **Step 1: Convert supported browser fixtures from `hybrid` to `authorization_code`; retain their redirect URIs and offline access settings.**
- [ ] **Step 2: Replace the password-grant token fixture with an approved-flow scenario, unless a dedicated legacy-exception test is required.
- [ ] **Step 3: Add documentation for the approved defaults, legacy configuration shape, rollout ordering, and the requirement to migrate EstateManagementUI and TransactionMobile.
- [ ] **Step 4: Regenerate Reqnroll `.feature.cs` files through the repository’s normal build/test command rather than hand-editing generated files.
- [ ] **Step 5: Commit** with `git commit -m "Update OAuth fixtures and rollout documentation"`.

### Task 7: Full verification and security review

**Files:**
- Review all changed files and generated outputs.

- [ ] **Step 1: Run formatting/diff checks:** `git diff --check`.
- [ ] **Step 2: Run unit tests:** `dotnet test SecurityService.UnitTests/SecurityService.UnitTests.csproj`.
- [ ] **Step 3: Run integration tests:** `dotnet test SecurityService.IntegrationTests/SecurityService.IntegrationTests.csproj` and the OpenID Connect integration test project when its environment is available.
- [ ] **Step 4: Verify no client secret, legacy exception, or unrelated generated artifact was added accidentally.
- [ ] **Step 5: Review the final diff for default-deny behavior, existing-client reconciliation, PKCE enforcement, and compatibility documentation.
- [ ] **Step 6: Commit any final corrections** with a focused message.
