namespace SecurityService.BusinessLogic.Requests{
    using System;
    using System.Collections.Generic;
    using MediatR;

    public class CreateClientRequest : IRequest{
        #region Constructors

        private CreateClientRequest(String clientId,
                                    String secret,
                                    String clientName,
                                    String clientDescription,
                                    List<String> allowedScopes,
                                    List<String> allowedGrantTypes,
                                    String clientUri,
                                    List<String> clientRedirectUris,
                                    List<String> clientPostLogoutRedirectUris,
                                    Boolean requireConsent,
                                    Boolean allowOfflineAccess){
            this.ClientId = clientId;
            this.Secret = secret;
            this.ClientName = clientName;
            this.ClientDescription = clientDescription;
            this.AllowedScopes = allowedScopes;
            this.AllowedGrantTypes = allowedGrantTypes;
            this.ClientUri = clientUri;
            this.ClientRedirectUris = clientRedirectUris;
            this.ClientPostLogoutRedirectUris = clientPostLogoutRedirectUris;
            this.RequireConsent = requireConsent;
            this.AllowOfflineAccess = allowOfflineAccess;
        }

        #endregion

        #region Properties

        public List<String> AllowedGrantTypes{ get; }
        public List<String> AllowedScopes{ get; }
        public Boolean AllowOfflineAccess{ get; }
        public String ClientDescription{ get; }
        public String ClientId{ get; }
        public String ClientName{ get; }
        public List<String> ClientPostLogoutRedirectUris{ get; }
        public List<String> ClientRedirectUris{ get; }
        public String ClientUri{ get; }
        public Boolean RequireConsent{ get; }
        public String Secret{ get; }

        #endregion

        #region Methods

        public static CreateClientRequest Create(String clientId, String secret, String clientName, String clientDescription, List<String> allowedScopes, List<String> allowedGrantTypes, String clientUri, List<String> clientRedirectUris, List<String> clientPostLogoutRedirectUris, Boolean requireConsent, Boolean allowOfflineAccess){
            return new CreateClientRequest(clientId, secret, clientName, clientDescription, allowedScopes, allowedGrantTypes, clientUri, clientRedirectUris, clientPostLogoutRedirectUris, requireConsent, allowOfflineAccess);
        }

        #endregion
    }
}