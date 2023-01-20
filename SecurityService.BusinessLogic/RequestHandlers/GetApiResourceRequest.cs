using System;
using Duende.IdentityServer.Models;
using MediatR;

namespace SecurityService.BusinessLogic.RequestHandlers;

public class GetApiResourceRequest : IRequest<ApiResource>
{
    public String Name { get; }

    public GetApiResourceRequest(String name)
    {
        Name = name;
    }

    public static GetApiResourceRequest Create(String name)
    {
        return new GetApiResourceRequest(name);
    }
}