using MessagingService.Client;
using MessagingService.DataTransferObjects;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleResults;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SecurityService.BusinessLogic;

public class ServiceOptions
{
    public ServiceOptions()
    {
        this.PasswordOptions = new PasswordOptions();
        this.KestrelOptions = new KestrelOptions();
        this.TokenOptions = new TokenOptions();
        this.SignInOptions = new SignInOptions();
        this.UserOptions = new UserOptions();
    }

    #region Properties

    public bool SeedDefaultScopes { get; set; } = true;

    public string InMemoryDatabaseName { get; set; } = "NewSecurityService";

    public String ClientId { get; set; } = string.Empty;

    public String ClientSecret { get; set; } = string.Empty;

    public String IssuerUrl { get; set; } = string.Empty;

    public KestrelOptions KestrelOptions { get; set; }

    public PasswordOptions PasswordOptions { get; set; }

    public String PublicOrigin { get; set; } = string.Empty;

    public SignInOptions SignInOptions { get; set; }

    public TokenOptions TokenOptions { get; set; }

    public Boolean UseInMemoryDatabase { get; set; }

    public UserOptions UserOptions { get; set; }

    #endregion
}

public class KestrelOptions
{
    public bool AutoDiscoverFromContentRoot { get; set; }

    public string Password { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string SearchPattern { get; set; } = "*.pfx";
}

public class TokenOptions
{
    #region Properties

    public Int32 EmailConfirmationTokenExpiryInHours { get; set; }

    public Int32 PasswordResetTokenExpiryInHours { get; set; }

    #endregion
}

public class UserOptions
{
    #region Properties

    public Boolean RequireUniqueEmail { get; set; }

    #endregion
}

public class SignInOptions
{
    #region Properties

    public Boolean RequireConfirmedEmail { get; set; }

    #endregion
}

public class PasswordOptions
{
    #region Properties

    public Boolean RequireDigit { get; set; }

    public Int32 RequiredLength { get; set; }

    public Boolean RequireUppercase { get; set; }
    public Boolean RequireLowercase { get; set; }
    public Boolean RequireNonAlphanumeric { get; set; }
    public Int32 RequiredUniqueChars { get; set; }


    #endregion
}



public class TestMessagingServiceClient : IMessagingServiceClient
{
    public SendEmailRequest LastEmailRequest { get; private set; }
    public SendSMSRequest LastSMSRequest { get; private set; }

    public async Task<Result> ResendEmail(string accessToken, ResendEmailRequest request, CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> SendEmail(String accessToken,
                                        SendEmailRequest request,
                                        CancellationToken cancellationToken)
    {
        //Logger.LogWarning($"Sending Email {request.Subject}");
        this.LastEmailRequest = new SendEmailRequest() {
            Body = request.Body,
            ConnectionIdentifier = request.ConnectionIdentifier,
            EmailAttachments = request.EmailAttachments,
            FromAddress = request.FromAddress,
            IsHtml = request.IsHtml,
            MessageId = Guid.NewGuid(),
            Subject = request.Subject,
            ToAddresses = request.ToAddresses
        };
        return Result.Success();
    }

    public async Task<Result> SendSMS(String accessToken,
                                      SendSMSRequest request,
                                      CancellationToken cancellationToken)
    {
        this.LastSMSRequest = request;
        return Result.Success();
    }
}

public interface IClientJwtService
{
    String CreateClientAssertion(string clientId, string issuer, Int32 expiresInMinutes);
}

public sealed class ClientJwtService : IClientJwtService
{
    private readonly SigningCredentials _credentials;

    public ClientJwtService(SigningCredentials credentials)
    {
        _credentials = credentials;
    }
    public string CreateClientAssertion(string clientId, string issuer, Int32 expiresInMinutes)
    {
        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer + "/connect/token",
            claims: new[]
            {
                new Claim("sub", clientId),
                new Claim("jti", Guid.NewGuid().ToString())
            },
            notBefore: now,
            expires: now.AddMinutes(expiresInMinutes),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}