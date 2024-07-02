using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityService.Models
{
    public class ChangeUserPasswordResult
    {
        public Boolean IsSuccessful { get; set; }
        public String RedirectUri { get; set; }
    }
}
