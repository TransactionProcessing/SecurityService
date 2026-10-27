# Access token claims

Access tokens issued by the SecurityService token endpoint include the `client_type` claim. Resource services can use it to distinguish a token representing a user from one representing a service client.

| Claim | Value | Meaning |
| --- | --- | --- |
| `client_type` | `user` | A token issued for a user, including authorization-code, refresh-token, and enabled password-grant flows. |
| `client_type` | `client` | A token issued through the client-credentials grant for a service client. |

The claim is included in access tokens. Its values are lowercase and form part of the token contract; consumers should handle unknown future values safely.
