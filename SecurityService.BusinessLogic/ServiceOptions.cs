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