using System;
using Duende.IdentityServer.Models;
using MediatR;

namespace SecurityService.BusinessLogic.Requests;

public class GetApiScopeRequest : IRequest<ApiScope>
{
    public String Name { get; }

    public GetApiScopeRequest(String name)
    {
        Name = name;
    }

    public static GetApiScopeRequest Create(String name)
    {
        return new GetApiScopeRequest(name);
    }
}