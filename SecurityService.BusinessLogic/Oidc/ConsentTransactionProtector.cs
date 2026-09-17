using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace SecurityService.BusinessLogic.Oidc;

public sealed record ConsentTransaction(
    string UserId,
    string AuthorizationUrl,
    string ClientId,
    IReadOnlyCollection<string> RequestedScopes,
    DateTimeOffset ExpiresAt);

public sealed class ConsentTransactionProtector
{
    private readonly IDataProtector _protector;

    public ConsentTransactionProtector(IDataProtector protector)
    {
        this._protector = protector;
    }

    public string Protect(ConsentTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        return this._protector.Protect(JsonSerializer.Serialize(transaction));
    }

    public ConsentTransaction? Unprotect(string? token, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var transaction = JsonSerializer.Deserialize<ConsentTransaction>(this._protector.Unprotect(token));
            return transaction is { ExpiresAt: var expiresAt } && expiresAt > now
                ? transaction
                : null;
        }
        catch (Exception exception) when (exception is FormatException or JsonException or CryptographicException)
        {
            return null;
        }
    }
}
