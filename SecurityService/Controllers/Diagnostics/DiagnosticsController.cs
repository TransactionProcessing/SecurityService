namespace SecurityService.Controllers.Diagnostics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using ViewModels;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(DiagnosticsController.ControllerRoute)]
    [ExcludeFromCodeCoverage]
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {
        #region Methods

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns></returns>
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var localAddresses = new[] {"127.0.0.1", "::1", this.HttpContext.Connection.LocalIpAddress.ToString()};
            if (!localAddresses.Contains(this.HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                return this.NotFound();
            }

            var model = new DiagnosticsViewModel(await this.HttpContext.AuthenticateAsync());
            return this.View(model);
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "diagnostics";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = DiagnosticsController.ControllerName;

        #endregion
    }
}