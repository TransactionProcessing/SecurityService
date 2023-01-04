namespace SecurityService.BusinessLogic;

using System;

public class PasswordOptions
{
    #region Properties

    public Boolean RequireDigit { get; set; }

    public Int32 RequiredLength { get; set; }

    public Boolean RequireUpperCase { get; set; }

    #endregion
}