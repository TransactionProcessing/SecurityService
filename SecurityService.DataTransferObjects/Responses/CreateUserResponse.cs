namespace SecurityService.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class CreateUserResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid UserId { get; set; }

        #endregion
    }
}