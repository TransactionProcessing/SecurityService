namespace SecurityService.BusinessLogic.Requests{
    using System;
    using System.Collections.Generic;
    using MediatR;

    public class CreateIdentityResourceRequest : IRequest<Unit>{
        #region Constructors

        private CreateIdentityResourceRequest(String name,
                                              String displayName,
                                              String description,
                                              Boolean required,
                                              Boolean emphasize,
                                              Boolean showInDiscoveryDocument,
                                              List<String> claims){
            this.Name = name;
            this.DisplayName = displayName;
            this.Description = description;
            this.Required = required;
            this.Emphasize = emphasize;
            this.ShowInDiscoveryDocument = showInDiscoveryDocument;
            this.Claims = claims;
        }

        #endregion

        #region Properties

        public List<String> Claims{ get; }
        public String Description{ get; }
        public String DisplayName{ get; }
        public Boolean Emphasize{ get; }
        public String Name{ get; }
        public Boolean Required{ get; }
        public Boolean ShowInDiscoveryDocument{ get; }

        #endregion

        #region Methods

        public static CreateIdentityResourceRequest Create(String name, String displayName, String description, Boolean required, Boolean emphasize, Boolean showInDiscoveryDocument, List<String> claims){
            return new CreateIdentityResourceRequest(name, displayName, description, required, emphasize, showInDiscoveryDocument, claims);
        }

        #endregion
    }
}