namespace SecurityService.BusinessLogic.Requests;

using System;
using MediatR;
using Models;

public class GetUserRequest : IRequest<UserDetails>{
    #region Constructors

    private GetUserRequest(Guid userId){
        this.UserId = userId;
    }

    #endregion

    #region Properties

    public Guid UserId{ get; }

    #endregion

    #region Methods

    public static GetUserRequest Create(Guid userId){
        return new GetUserRequest(userId);
    }

    #endregion
}