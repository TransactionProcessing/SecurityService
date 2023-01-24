namespace SecurityService.BusinessLogic.Requests;

using System;
using MediatR;

public class CreateApiScopeRequest : IRequest<Unit>{
    #region Constructors

    public CreateApiScopeRequest(String name, String displayName, String description){
        this.Name = name;
        this.DisplayName = displayName;
        this.Description = description;
    }

    #endregion

    #region Properties

    public String Description{ get; }
    public String DisplayName{ get; }

    public String Name{ get; }

    #endregion

    #region Methods

    public static CreateApiScopeRequest Create(String name, String displayName, String description){
        return new CreateApiScopeRequest(name, displayName, description);
    }

    #endregion
}