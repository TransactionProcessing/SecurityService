using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecurityServiceTestUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityServiceTestUI.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;

    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult ChangePassword() {
            String redirectUri = $"{_configuration.GetValue<String>("AppSettings:Authority")}/Account/ChangePassword?clientId={_configuration.GetValue<String>("AppSettings:ClientId")}";

            return this.Redirect(redirectUri);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
