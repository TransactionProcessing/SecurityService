using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.BusinessLogic.Requests
{
    using MediatR;

    public class ConfirmUserEmailAddressRequest : IRequest<Boolean>
    {
        public String UserName{ get; }
        public String ConfirmEmailToken{ get; }

        private ConfirmUserEmailAddressRequest(String userName,
                                              String confirmEmailToken){
            this.UserName = userName;
            this.ConfirmEmailToken = confirmEmailToken;
        }

        public static ConfirmUserEmailAddressRequest Create(String userName, String confirmEmailToken){
            return new ConfirmUserEmailAddressRequest(userName, confirmEmailToken);
        }
    }
}
