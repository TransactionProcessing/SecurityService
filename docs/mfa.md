# Multi-factor authentication

SecurityService provides local-account MFA using an authenticator application and one-time recovery codes.

## Enrollment

An authenticated user opens `/Account/ManageMfa`, chooses **Begin enrollment**, scans or imports the displayed `otpauth://` URI into an authenticator application, and confirms the six-digit code. MFA is not enabled until confirmation succeeds.

The supported second factors are:

- TOTP from a standards-compatible authenticator application.
- Recovery codes generated from the recovery-code page. Each code is displayed once, stored as a hash, and consumed at most once.

SMS, email, push notifications, and WebAuthn are not part of this initial implementation.

## Policy management

Administrators can require or remove MFA through the existing management API:

- `PUT` or `DELETE` `/api/users/{userId}/mfa-policy`
- `PUT` or `DELETE` `/api/roles/{roleId}/mfa-policy`

Policies control whether MFA is required; they do not enroll a user or generate an authenticator secret. A role policy applies to every current member of that role at login time.

## Login behavior

Local password login validates the password first and then issues a short-lived, protected MFA challenge. The application cookie is issued only after a valid TOTP or recovery code. Failed second-factor attempts participate in Identity lockout.

The password grant is non-interactive and cannot complete MFA. It returns OAuth `invalid_grant` for accounts with MFA enabled or an applicable MFA policy. Clients should use the interactive authorization-code flow, which redirects through SecurityService's hosted login and MFA pages.

Client applications should link authenticated users to the hosted account page rather than duplicating the enrollment screens. A typical link is `/Account/ManageMfa` on the SecurityService authority.

## Migration and operations

Apply the generated EF migrations before enabling MFA policies. The MFA tables are `MfaPolicies` and `RecoveryCodes`; recovery codes are never included in claims or logs.

For a multi-instance deployment, set `ServiceOptions:DataProtectionKeyDirectory` to a shared, access-controlled directory. This preserves the protected MFA challenge and Identity tokens across restarts and instances. The directory must not be a user-writable public web directory.

Removing an authenticator requires the current password or a valid current TOTP code, disables Identity two-factor authentication, resets the authenticator key, and revokes all recovery codes. Users should generate a fresh set after re-enrollment.
