namespace SecurityService.BusinessLogic;

using System;

public class TokenOptions
{
    #region Properties

    public Int32 EmailConfirmationTokenExpiryInHours { get; set; }

    public Int32 PasswordResetTokenExpiryInHours { get; set; }

    #endregion
}