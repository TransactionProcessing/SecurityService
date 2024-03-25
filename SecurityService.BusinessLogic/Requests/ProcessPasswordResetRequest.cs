using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.Requests
{
    using MediatR;

    public  class ProcessPasswordResetRequest : IRequest
    {
        public String Username{ get; }
        public String EmailAddress{ get; }
        public String ClientId{ get; }

        private ProcessPasswordResetRequest(String username,
                                           String emailAddress,
                                           String clientId){
            this.Username = username;
            this.EmailAddress = emailAddress;
            this.ClientId = clientId;
        }

        public static ProcessPasswordResetRequest Create(String username, String emailAddress, String clientId){
            return new ProcessPasswordResetRequest(username, emailAddress, clientId);
        }
    }
}
