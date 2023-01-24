namespace SecurityService.BusinessLogic.Requests;

using System;
using Duende.IdentityServer.Models;
using MediatR;

public class GetIdentityResourceRequest : IRequest<IdentityResource>{
    #region Constructors

    private GetIdentityResourceRequest(String identityResourceName){
        this.IdentityResourceName = identityResourceName;
    }

    #endregion

    #region Properties

    public String IdentityResourceName{ get; }

    #endregion

    #region Methods

    public static GetIdentityResourceRequest Create(String identityResourceName){
        return new GetIdentityResourceRequest(identityResourceName);
    }

    #endregion
}