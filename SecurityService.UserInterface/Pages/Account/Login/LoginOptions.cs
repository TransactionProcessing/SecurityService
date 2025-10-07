namespace IdentityServerHost.Pages.Login;

using System;

public class LoginOptions
{
    public readonly bool AllowLocalLogin = true;
    public readonly bool AllowRememberLogin = true;
    public readonly TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);
    public readonly string InvalidCredentialsErrorMessage = "Invalid username or password";
}