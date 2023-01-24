namespace SecurityService.BusinessLogic.Requests;

using System;
using System.Collections.Generic;
using Duende.IdentityServer.Models;
using MediatR;

public class GetIdentityResourcesRequest : IRequest<List<IdentityResource>>{
    #region Constructors

    private GetIdentityResourcesRequest(){
    }

    #endregion

    #region Methods

    public static GetIdentityResourcesRequest Create(){
        return new GetIdentityResourcesRequest();
    }

    #endregion
}