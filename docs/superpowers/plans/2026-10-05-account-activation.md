# Account activation instead of emailed passwords

## Goal

Ensure emailed user passwords are never generated, persisted for normal email-backed users, or included in email payloads. New users receive a short-lived Identity password token and choose their password through `/Account/ActivateAccount`.

## Tasks

1. Add failing unit tests for activation email content, activation processing, and user creation without a password hash.
2. Change `UserRequestHandler` to create email-backed users without a password, send activation links, and resend either email confirmation or activation depending on account state.
3. Add the activation command and Razor Page; use `ResetPasswordAsync` so Identity enforces token expiry and single-use semantics.
4. Update existing confirmation, resend, unit, and integration expectations from welcome-password behavior to activation behavior.
5. Run focused tests, the complete solution test suite, and build verification.

## Design decisions

- Existing authentication/token endpoints remain unchanged.
- The existing password-reset data-protection provider and configured two-hour lifetime are reused for activation tokens; successful password reset rotates the security stamp, making the token single-use.
- Users without an email address retain the legacy direct-password path because there is no email channel through which to activate them.
- The public `CreateUserRequest.Password` property remains temporarily for compatibility, but is ignored for email-backed users and never returned or emailed.

## Review focus

- No plaintext password reaches an email builder, log message, result, or response.
- Invalid, expired, and reused activation tokens fail without changing the password.
- Existing forgot-password and login flows remain compatible.
