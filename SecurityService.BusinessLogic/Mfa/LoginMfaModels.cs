using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace SecurityService.BusinessLogic.Mfa;

public abstract record LoginOutcome;
public sealed record LoginCompleted(string ReturnUrl = "/") : LoginOutcome;
public sealed record LoginRequiresMfa(string Transaction) : LoginOutcome;
public sealed record LoginRejected(string Message) : LoginOutcome;

public sealed record MfaSignInTransaction(string UserId, string ReturnUrl, bool RememberLogin, DateTimeOffset ExpiresUtc);

public sealed class MfaSignInTransactionProtector
{
    private readonly IDataProtector _protector;

    public MfaSignInTransactionProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("SecurityService.MfaSignInTransaction.v1");
    }

    public string Protect(string userId, string returnUrl, bool rememberLogin)
    {
        var transaction = new MfaSignInTransaction(userId, returnUrl, rememberLogin, DateTimeOffset.UtcNow.AddMinutes(5));
        var payload = JsonSerializer.SerializeToUtf8Bytes(transaction);
        return WebEncoders.Base64UrlEncode(_protector.Protect(payload));
    }

    public bool TryUnprotect(string value, out MfaSignInTransaction? transaction)
    {
        transaction = null;
        try
        {
            var payload = _protector.Unprotect(WebEncoders.Base64UrlDecode(value));
            transaction = JsonSerializer.Deserialize<MfaSignInTransaction>(payload);
            return transaction is not null && transaction.ExpiresUtc > DateTimeOffset.UtcNow;
        }
        catch (Exception) when (value.Length > 0)
        {
            return false;
        }
    }
}
