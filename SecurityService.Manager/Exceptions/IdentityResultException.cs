namespace SecurityService.Manager.Exceptions
{
    using System;
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Identity;

    public class IdentityResultException : Exception
    {
        #region Public Properties

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public String Error { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityResultException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="identityResult">The identity result.</param>
        public IdentityResultException(String message, IdentityResult identityResult) : base(message)
        {
            StringBuilder errors = new StringBuilder();
            if (identityResult.Errors.Any())
            {
                // If there are any Errors then build up the error message
                foreach (var identityError in identityResult.Errors)
                {
                    errors.AppendLine(identityError.Description);
                }
            }

            this.Error = errors.ToString();
        }

        #endregion
    }
}