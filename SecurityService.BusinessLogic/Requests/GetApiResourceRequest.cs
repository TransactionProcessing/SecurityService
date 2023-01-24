using System;
using Duende.IdentityServer.Models;
using MediatR;

namespace SecurityService.BusinessLogic.Requests;

public class GetApiResourceRequest : IRequest<ApiResource>
{
    public string Name { get; }

    public GetApiResourceRequest(string name)
    {
        Name = name;
    }

    public static GetApiResourceRequest Create(string name)
    {
        return new GetApiResourceRequest(name);
    }
}