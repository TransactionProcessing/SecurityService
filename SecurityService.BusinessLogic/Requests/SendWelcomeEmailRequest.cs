using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.Requests
{
    using MediatR;

    public class SendWelcomeEmailRequest : IRequest
    {
        public String UserName{ get; }

        private SendWelcomeEmailRequest(String userName){
            this.UserName = userName;
        }

        public static SendWelcomeEmailRequest Create(String userName){
            return new SendWelcomeEmailRequest(userName);
        }
    }
}
