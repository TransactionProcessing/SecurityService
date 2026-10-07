using SecurityService.BusinessLogic.Validation;
using Shouldly;
using SimpleResults;

namespace SecurityService.UnitTests.RequestHandlers;

public sealed class ClientRedirectUriValidatorTests
{
    [Fact]
    public void Validate_AllowsHttpsUri()
    {
        var result = ClientRedirectUriValidator.Validate(
            ["https://client.example/signin-oidc"],
            "ClientRedirectUris");

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldHaveSingleItem();
        result.Data![0].OriginalString.ShouldBe("https://client.example/signin-oidc");
    }

    [Theory]
    [InlineData("http://localhost/signin-oidc")]
    [InlineData("http://127.0.0.1/signin-oidc")]
    [InlineData("http://[::1]/signin-oidc")]
    public void Validate_AllowsHttpLoopbackUri(string value)
    {
        var result = ClientRedirectUriValidator.Validate([value], "ClientRedirectUris");

        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("http://client.example/signin-oidc")]
    [InlineData("ftp://client.example/signin-oidc")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/plain,unsafe")]
    public void Validate_RejectsNonHttpsOrUnsafeSchemes(string value)
    {
        var result = ClientRedirectUriValidator.Validate([value], "ClientRedirectUris");

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData("client.example/signin-oidc")]
    [InlineData("https://*.client.example/signin-oidc")]
    [InlineData("https://user:password@client.example/signin-oidc")]
    [InlineData("https://client.example/signin-oidc#fragment")]
    [InlineData("https://client.example/signin-oidc?iss=attacker")]
    [InlineData("not a uri")]
    public void Validate_RejectsMalformedOrUnsafeUri(string value)
    {
        var result = ClientRedirectUriValidator.Validate([value], "ClientPostLogoutRedirectUris");

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Message.ShouldContain("ClientPostLogoutRedirectUris");
    }

    [Fact]
    public void Validate_IgnoresBlankValuesAndRemovesDuplicates()
    {
        var result = ClientRedirectUriValidator.Validate(
            ["", "  ", "https://client.example/callback", "HTTPS://CLIENT.EXAMPLE/callback"],
            "ClientRedirectUris");

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldHaveSingleItem();
    }
}
