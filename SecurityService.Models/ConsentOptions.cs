namespace SecurityService.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConsentOptions
    {
        #region Fields

        /// <summary>
        /// The enable offline access
        /// </summary>
        public static Boolean EnableOfflineAccess = true;

        /// <summary>
        /// The invalid selection error message
        /// </summary>
        public static readonly String InvalidSelectionErrorMessage = "Invalid selection";

        /// <summary>
        /// The must choose one error message
        /// </summary>
        public static readonly String MustChooseOneErrorMessage = "You must pick at least one permission";

        /// <summary>
        /// The offline access description
        /// </summary>
        public static String OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

        /// <summary>
        /// The offline access display name
        /// </summary>
        public static String OfflineAccessDisplayName = "Offline Access";

        #endregion
    }
}