namespace SecurityService.BusinessLogic;

using System;

public class ServiceOptions
{
    public ServiceOptions() {
        this.PasswordOptions = new PasswordOptions();
        this.TokenOptions = new TokenOptions();
        this.SignInOptions = new SignInOptions();
        this.UserOptions = new UserOptions();
    }

    #region Properties
    
    public String ClientId { get; set; }

    public String ClientSecret { get; set; }

    public String IssuerUrl { get; set; }

    public PasswordOptions PasswordOptions { get; set; }

    public String PublicOrigin { get; set; }

    public SignInOptions SignInOptions { get; set; }

    public TokenOptions TokenOptions { get; set; }

    public Boolean UseInMemoryDatabase { get; set; }

    public UserOptions UserOptions { get; set; }

    #endregion
}

public class UserOptions
{
    #region Properties

    public Boolean RequireUniqueEmail { get; set; }

    #endregion
}

public class TokenOptions
{
    #region Properties

    public Int32 EmailConfirmationTokenExpiryInHours { get; set; }

    public Int32 PasswordResetTokenExpiryInHours { get; set; }

    #endregion
}

public class SignInOptions
{
    #region Properties

    public Boolean RequireConfirmedEmail { get; set; }

    #endregion
}

public class PasswordOptions
{
    #region Properties

    public Boolean RequireDigit { get; set; }

    public Int32 RequiredLength { get; set; }

    public Boolean RequireUpperCase { get; set; }

    #endregion
}