namespace SecurityService.BusinessLogic.Requests;

using System.Collections.Generic;
using MediatR;
using Models;

public class GetRolesRequest : IRequest<List<RoleDetails>>{
    #region Constructors

    private GetRolesRequest(){
    }

    #endregion

    #region Methods

    public static GetRolesRequest Create(){
        return new GetRolesRequest();
    }

    #endregion
}