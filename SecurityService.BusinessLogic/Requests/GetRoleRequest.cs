namespace SecurityService.BusinessLogic.Requests;

using System;
using MediatR;
using Models;

public class GetRoleRequest : IRequest<RoleDetails>{
    #region Constructors

    private GetRoleRequest(Guid roleId){
        this.RoleId = roleId;
    }

    #endregion

    #region Properties

    public Guid RoleId{ get; }

    #endregion

    #region Methods

    public static GetRoleRequest Create(Guid roleId){
        return new GetRoleRequest(roleId);
    }

    #endregion
}