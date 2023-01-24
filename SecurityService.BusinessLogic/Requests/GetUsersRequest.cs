namespace SecurityService.BusinessLogic.Requests;

using System;
using System.Collections.Generic;
using MediatR;
using Models;

public class GetUsersRequest : IRequest<List<UserDetails>>{
    public String UserName{ get; }

    #region Fields

    #endregion

    #region Constructors

    private GetUsersRequest(String userName){
        this.UserName = userName;
    }

    #endregion

    #region Methods

    public static GetUsersRequest Create(String userName){
        return new GetUsersRequest(userName);
    }

    #endregion
}