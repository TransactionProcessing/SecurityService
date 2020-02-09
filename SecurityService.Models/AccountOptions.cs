namespace SecurityService.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AccountOptions
    {
        #region Fields

        /// <summary>
        /// The allow local login
        /// </summary>
        public static Boolean AllowLocalLogin = true;

        /// <summary>
        /// The allow remember login
        /// </summary>
        public static Boolean AllowRememberLogin = true;

        /// <summary>
        /// The automatic redirect after sign out
        /// </summary>
        public static Boolean AutomaticRedirectAfterSignOut = false;

        // if user uses windows auth, should we load the groups from windows
        /// <summary>
        /// The include windows groups
        /// </summary>
        public static Boolean IncludeWindowsGroups = false;

        /// <summary>
        /// The invalid credentials error message
        /// </summary>
        public static String InvalidCredentialsErrorMessage = "Invalid username or password";

        /// <summary>
        /// The remember me login duration
        /// </summary>
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        /// <summary>
        /// The show logout prompt
        /// </summary>
        public static Boolean ShowLogoutPrompt = true;

        // specify the Windows authentication scheme being used
        /// <summary>
        /// The windows authentication scheme name
        /// </summary>
        public static readonly String WindowsAuthenticationSchemeName = "Windows";

        #endregion
    }
}