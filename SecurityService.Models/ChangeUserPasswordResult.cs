namespace SecurityService.Models;

public class ChangeUserPasswordResult
{
    public Boolean IsSuccessful { get; set; }
    public String RedirectUri { get; set; }
}