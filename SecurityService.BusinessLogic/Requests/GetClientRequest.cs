namespace SecurityService.BusinessLogic.Requests;

using System;
using Duende.IdentityServer.Models;
using MediatR;

public class GetClientRequest : IRequest<Client>{
    #region Constructors

    private GetClientRequest(String clientId){
        this.ClientId = clientId;
    }

    #endregion

    #region Properties

    public String ClientId{ get; }

    #endregion

    #region Methods

    public static GetClientRequest Create(String clientId){
        return new GetClientRequest(clientId);
    }

    #endregion
}