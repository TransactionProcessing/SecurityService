using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.Requests
{
    using MediatR;

    public class ChangeUserPasswordRequest :IRequest<(Boolean,String)>
    {
        public String UserName{ get; }
        public String CurrentPassword{ get; }
        public String NewPassword{ get; }
        public String ClientId{ get; }

        private ChangeUserPasswordRequest(String userName,
                                         String currentPassword,
                                         String newPassword,
                                         String clientId){
            this.UserName = userName;
            this.CurrentPassword = currentPassword;
            this.NewPassword = newPassword;
            this.ClientId = clientId;
        }

        public static ChangeUserPasswordRequest Create(String userName, String currentPassword, String newPassword, String clientId){
            return new ChangeUserPasswordRequest(userName, currentPassword, newPassword, clientId);
        }
    }
}
