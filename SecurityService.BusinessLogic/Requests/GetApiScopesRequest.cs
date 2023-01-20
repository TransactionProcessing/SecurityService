using System.Collections.Generic;
using Duende.IdentityServer.Models;
using MediatR;

namespace SecurityService.BusinessLogic.Requests;

public class GetApiScopesRequest : IRequest<List<ApiScope>>
{
    public GetApiScopesRequest()
    {
    }

    public static GetApiScopesRequest Create()
    {
        return new GetApiScopesRequest();
    }
}