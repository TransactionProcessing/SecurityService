# OAuth grant restrictions

The default server capabilities are authorization code with PKCE, client credentials, and refresh tokens. Password, implicit, and hybrid grants are disabled unless a deployment explicitly lists the client IDs that still require them. Device authorization is not enabled.

Legacy exceptions are configured under `ServiceOptions:OAuth:LegacyGrantTypeClients`:

```json
{
  "ServiceOptions": {
    "OAuth": {
      "LegacyGrantTypeClients": {
        "password": [ "legacy-mobile-client" ],
        "hybrid": [ "legacy-web-client" ]
      }
    }
  }
}
```

The exception list controls both server capability registration and client creation validation. Existing client records are reconciled during database initialization. Each legacy client should therefore be explicitly listed before deployment, and the downstream client should be migrated to authorization code with PKCE or client credentials where possible.
