using System;
using MediatR;

namespace SecurityService.BusinessLogic.Requests;

public class CreateApiScopeRequest : IRequest<Unit>
{
    public CreateApiScopeRequest(String name, String displayName,String description)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
    }

    public String Name { get; }
    public String DisplayName { get; }
    public String Description { get; }

    public static CreateApiScopeRequest Create(String name, String displayName, String description)
    {
        return new CreateApiScopeRequest(name, displayName, description);
    }
        
}