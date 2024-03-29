﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
            String redirectUri = $"{Startup.Configuration.GetValue<String>("AppSettings:Authority")}/Account/ChangePassword?clientId={Startup.Configuration.GetValue<String>("AppSettings:ClientId")}";

            return this.Redirect(redirectUri);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
