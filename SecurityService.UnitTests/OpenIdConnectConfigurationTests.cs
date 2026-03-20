namespace SecurityService.UnitTests;

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SecurityServiceTestUI;
using Shouldly;
using Xunit;

public class OpenIdConnectConfigurationTests
{
    [Fact]
    public void GetIssuerUrl_WhenIssuerOverrideConfigured_ReturnsOverride()
    {
        IConfiguration configuration = BuildConfiguration(
            ("AppSettings:Authority", "https://keycloak-internal:8443/realms/security"),
            ("AppSettings:IssuerUrl", "https://issuer.example.com/realms/security"));

        String issuerUrl = OpenIdConnectConfiguration.GetIssuerUrl(configuration);

        issuerUrl.ShouldBe("https://issuer.example.com/realms/security");
    }

    [Fact]
    public void GetIssuerUrl_WhenIssuerOverrideMissing_FallsBackToAuthority()
    {
        IConfiguration configuration = BuildConfiguration(
            ("AppSettings:Authority", "https://keycloak-internal:8443/realms/security"));

        String issuerUrl = OpenIdConnectConfiguration.GetIssuerUrl(configuration);

        issuerUrl.ShouldBe("https://keycloak-internal:8443/realms/security");
    }

    [Fact]
    public void GetMetadataAddress_WhenOverrideMissing_BuildsWellKnownEndpointFromAuthority()
    {
        IConfiguration configuration = BuildConfiguration(
            ("AppSettings:Authority", "https://keycloak-internal:8443/realms/security"));

        String metadataAddress = OpenIdConnectConfiguration.GetMetadataAddress(configuration);

        metadataAddress.ShouldBe("https://keycloak-internal:8443/realms/security/.well-known/openid-configuration");
    }

    [Fact]
    public void GetMetadataAddress_WhenOverrideConfigured_ReturnsOverride()
    {
        IConfiguration configuration = BuildConfiguration(
            ("AppSettings:Authority", "https://keycloak-internal:8443/realms/security"),
            ("AppSettings:MetadataAddress", "https://metadata.example.com/.well-known/openid-configuration"));

        String metadataAddress = OpenIdConnectConfiguration.GetMetadataAddress(configuration);

        metadataAddress.ShouldBe("https://metadata.example.com/.well-known/openid-configuration");
    }

    [Fact]
    public void GetAuthorizationEndpointOverride_WhenConfigured_ReturnsOverride()
    {
        IConfiguration configuration = BuildConfiguration(
            ("AppSettings:AuthorizationEndpoint", "https://issuer.example.com/realms/security/protocol/openid-connect/auth"));

        String authorizationEndpoint = OpenIdConnectConfiguration.GetAuthorizationEndpointOverride(configuration);

        authorizationEndpoint.ShouldBe("https://issuer.example.com/realms/security/protocol/openid-connect/auth");
    }

    private static IConfiguration BuildConfiguration(params (String Key, String Value)[] settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings.ToDictionary(s => s.Key, s => s.Value)).Build();
}
