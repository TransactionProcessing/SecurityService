using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SecurityService.OpenApi;

/// <summary>Adds discoverable query, form, and bearer-auth examples for OIDC operations in Scalar.</summary>
public sealed class OidcRequestDocumentationTransformer : IOpenApiOperationTransformer
{
    private const string RedirectUri = "https://orders.example.com/signin-oidc";
    private const string CodeVerifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
    private const string CodeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        string path = context.Description.RelativePath?.TrimStart('/') ?? string.Empty;
        string method = context.Description.HttpMethod ?? string.Empty;

        if (path.Equals("connect/authorize", StringComparison.OrdinalIgnoreCase))
        {
            operation.OperationId = HttpMethods.IsPost(method) ? "AuthorizePost" : "AuthorizeGet";
            AddParameter(operation, "client_id", "Registered OAuth client identifier.", "orders-web", required: true);
            AddParameter(operation, "redirect_uri", "Must exactly match a callback URI registered for the client.", RedirectUri, required: true);
            AddParameter(operation, "response_type", "Use `code` for the authorization-code flow.", "code", required: true);
            AddParameter(operation, "scope", "Space-separated OIDC scopes and API permissions.", "openid profile orders.read", required: true);
            AddParameter(operation, "state", "Opaque value returned unchanged to the client callback.", "order-login-7f3a", required: false);
            AddParameter(operation, "code_challenge", "Base64url-encoded SHA-256 challenge derived from the PKCE verifier.", CodeChallenge, required: false);
            AddParameter(operation, "code_challenge_method", "Use `S256` for PKCE.", "S256", required: false);

            if (HttpMethods.IsPost(method))
            {
                SetFormRequest(operation, "Authorization request (form POST)", new Dictionary<string, string>
                {
                    ["client_id"] = "orders-web",
                    ["redirect_uri"] = RedirectUri,
                    ["response_type"] = "code",
                    ["scope"] = "openid profile orders.read",
                    ["state"] = "order-login-7f3a",
                    ["code_challenge"] = CodeChallenge,
                    ["code_challenge_method"] = "S256"
                });
            }
        }
        else if (path.Equals("connect/token", StringComparison.OrdinalIgnoreCase))
        {
            AddTokenForm(operation);
        }
        else if (path.Equals("connect/logout", StringComparison.OrdinalIgnoreCase))
        {
            operation.OperationId = HttpMethods.IsPost(method) ? "LogoutPost" : "LogoutGet";
            AddParameter(operation, "id_token_hint", "Previously issued ID token for the session being ended.", "eyJhbGciOi...", required: false);
            AddParameter(operation, "post_logout_redirect_uri", "Must exactly match a post-logout URI registered for the client.", "https://orders.example.com/signed-out", required: false);
            AddParameter(operation, "state", "Opaque value returned unchanged to the post-logout redirect URI.", "logout-4c82", required: false);

            if (HttpMethods.IsPost(method))
            {
                SetFormRequest(operation, "End-session request (form POST)", new Dictionary<string, string>
                {
                    ["id_token_hint"] = "eyJhbGciOi...",
                    ["post_logout_redirect_uri"] = "https://orders.example.com/signed-out",
                    ["state"] = "logout-4c82"
                });
            }
        }
        else if (path.Equals("connect/userinfo", StringComparison.OrdinalIgnoreCase))
        {
            operation.OperationId = HttpMethods.IsPost(method) ? "UserInfoPost" : "UserInfoGet";
            AddBearerAuthentication(operation, context.Document);
        }

        return Task.CompletedTask;
    }

    private static void AddParameter(OpenApiOperation operation, string name, string description, string value, bool required)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();
        if (operation.Parameters.Any(parameter => parameter.Name == name && parameter.In == ParameterLocation.Query))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Description = description,
            Required = required,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String, Example = JsonValue.Create(value) },
            Example = JsonValue.Create(value)
        });
    }

    private static void AddTokenForm(OpenApiOperation operation)
    {
        OpenApiSchema schema = new()
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["grant_type"] = StringField("OAuth grant to exchange, such as authorization_code, client_credentials, or refresh_token."),
                ["client_id"] = StringField("Registered OAuth client identifier."),
                ["client_secret"] = StringField("Required for confidential clients using client authentication."),
                ["code"] = StringField("Authorization code returned to the registered callback."),
                ["redirect_uri"] = StringField("Callback URI used in the authorization request; must match exactly."),
                ["code_verifier"] = StringField("Original PKCE verifier corresponding to code_challenge."),
                ["scope"] = StringField("Space-separated permissions requested for the token."),
                ["refresh_token"] = StringField("Refresh token previously issued to this client.")
            }
        };

        OpenApiMediaType content = new()
        {
            Schema = schema,
            Examples = new Dictionary<string, IOpenApiExample>
            {
                ["authorization_code"] = FormExample("Authorization code with PKCE", new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = "orders-web",
                    ["code"] = "authorization-code-from-callback",
                    ["redirect_uri"] = RedirectUri,
                    ["code_verifier"] = CodeVerifier
                }),
                ["client_credentials"] = FormExample("Client credentials", new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = "orders-worker",
                    ["client_secret"] = "replace-with-client-secret",
                    ["scope"] = "orders.read"
                }),
                ["refresh_token"] = FormExample("Refresh an access token", new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = "orders-web",
                    ["refresh_token"] = "replace-with-refresh-token"
                })
            }
        };

        operation.RequestBody = new OpenApiRequestBody
        {
            Description = "Submit one of the grant-specific form examples. Client authentication requirements depend on the client registration.",
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType> { ["application/x-www-form-urlencoded"] = content }
        };
    }

    private static void SetFormRequest(OpenApiOperation operation, string description, Dictionary<string, string> values)
    {
        OpenApiSchema schema = new()
        {
            Type = JsonSchemaType.Object,
            Properties = values.Keys.ToDictionary(key => key, _ => (IOpenApiSchema)StringField("OIDC form parameter."))
        };
        OpenApiExample example = FormExample(description, values);
        operation.RequestBody = new OpenApiRequestBody
        {
            Description = description,
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                {
                    Schema = schema,
                    Examples = new Dictionary<string, IOpenApiExample> { ["Example"] = example }
                }
            }
        };
    }

    private static OpenApiSchema StringField(string description) => new()
    {
        Type = JsonSchemaType.String,
        Description = description
    };

    private static OpenApiExample FormExample(string summary, Dictionary<string, string> values)
    {
        JsonObject value = new();
        foreach ((string key, string fieldValue) in values)
        {
            value[key] = fieldValue;
        }

        return new OpenApiExample { Summary = summary, Value = value };
    }

    private static void AddBearerAuthentication(OpenApiOperation operation, OpenApiDocument document)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        if (!document.Components.SecuritySchemes.TryGetValue("Bearer", out IOpenApiSecurityScheme? bearer))
        {
            bearer = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Use an access token issued with the `openid` scope. Example header: `Authorization: Bearer eyJhbGciOi...`."
            };
            document.Components.SecuritySchemes["Bearer"] = bearer;
        }

        OpenApiSecuritySchemeReference bearerReference = new("Bearer", document);
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement { [bearerReference] = new List<string>() });
    }
}
