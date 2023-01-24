namespace SecurityService.BusinessLogic.Requests;

using System.Collections.Generic;
using Duende.IdentityServer.Models;
using MediatR;

public class GetClientsRequest : IRequest<List<Client>>{
    #region Constructors

    private GetClientsRequest(){
    }

    #endregion

    #region Methods

    public static GetClientsRequest Create(){
        return new GetClientsRequest();
    }

    #endregion
}