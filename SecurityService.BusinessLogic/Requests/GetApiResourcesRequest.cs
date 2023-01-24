using System.Collections.Generic;
using Duende.IdentityServer.Models;
using MediatR;

namespace SecurityService.BusinessLogic.Requests;

public class GetApiResourcesRequest : IRequest<List<ApiResource>>
{
    public GetApiResourcesRequest()
    {
    }

    public static GetApiResourcesRequest Create()
    {
        return new GetApiResourcesRequest();
    }
}