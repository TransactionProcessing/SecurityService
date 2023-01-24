namespace SecurityService.BusinessLogic.Requests{
    using System;
    using MediatR;

    public class CreateRoleRequest : IRequest<Unit>{
        #region Constructors

        private CreateRoleRequest(Guid roleId, String name){
            this.RoleId = roleId;
            this.Name = name;
        }

        #endregion

        #region Properties

        public String Name{ get; }
        public Guid RoleId{ get; }

        #endregion

        #region Methods

        public static CreateRoleRequest Create(Guid roleId, String name){
            return new CreateRoleRequest(roleId, name);
        }

        #endregion
    }
}