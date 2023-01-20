using System;
using System.Collections.Generic;
using MediatR;

namespace SecurityService.BusinessLogic.RequestHandlers;

public class CreateApiResourceRequest : IRequest<Unit>
{
    public string Name { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string Secret { get; }
    public List<string> Scopes { get; }
    public List<string> UserClaims { get; }

    private CreateApiResourceRequest(String name,
        String displayName,
        String description,
        String secret,
        List<String> scopes,
        List<String> userClaims)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Secret = secret;
        Scopes = scopes;
        UserClaims = userClaims;
    }

    public static CreateApiResourceRequest Create(string name, string displayName, string description, string secret, List<string> scopes, List<string> userClaims)
    {
        return new CreateApiResourceRequest(name, displayName, description, secret, scopes, userClaims);
    }
}