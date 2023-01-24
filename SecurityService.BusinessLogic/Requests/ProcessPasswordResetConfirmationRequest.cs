using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.Requests
{
    using MediatR;

    public class ProcessPasswordResetConfirmationRequest : IRequest<String>
    {
        public String Username{ get; }
        public String Token{ get; }
        public String Password{ get; }
        public String ClientId{ get; }

        private ProcessPasswordResetConfirmationRequest(String username,
                                                       String token,
                                                       String password,
                                                       String clientId){
            this.Username = username;
            this.Token = token;
            this.Password = password;
            this.ClientId = clientId;
        }

        public static ProcessPasswordResetConfirmationRequest Create(String username, String token, String password, String clientId){
            return new ProcessPasswordResetConfirmationRequest(username, token, password, clientId);
        }
    }
}
