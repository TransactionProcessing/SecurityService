namespace SecurityServiceTestUI;

using System;
using Microsoft.Extensions.Configuration;

public static class OpenIdConnectConfiguration
{
    public static String GetAuthority(IConfiguration configuration) => configuration.GetValue<String>("AppSettings:Authority");

    public static String GetAuthorizationEndpointOverride(IConfiguration configuration) =>
        configuration.GetValue<String>("AppSettings:AuthorizationEndpoint");

    public static String GetIssuerUrl(IConfiguration configuration)
    {
        String issuerUrl = configuration.GetValue<String>("AppSettings:IssuerUrl");
        return String.IsNullOrWhiteSpace(issuerUrl) ? GetAuthority(configuration) : issuerUrl;
    }

    public static String GetMetadataAddress(IConfiguration configuration)
    {
        String metadataAddress = configuration.GetValue<String>("AppSettings:MetadataAddress");
        if (String.IsNullOrWhiteSpace(metadataAddress) == false)
        {
            return metadataAddress;
        }

        String authority = GetAuthority(configuration)?.TrimEnd('/');
        return String.IsNullOrWhiteSpace(authority) ? null : $"{authority}/.well-known/openid-configuration";
    }
}
