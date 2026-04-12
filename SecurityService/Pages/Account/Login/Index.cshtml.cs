using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SecurityService.BusinessLogic.Requests;

namespace SecurityService.Pages.Account.Login;

public sealed class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        this._mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyCollection<ExternalProviderViewModel> Providers { get; private set; } = Array.Empty<ExternalProviderViewModel>();

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        this.Input.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

        var queryString = new Uri("https://dummy" + this.Input.ReturnUrl).Query;
        var query = QueryHelpers.ParseQuery(queryString);
        this.Input.ClientId = query["client_id"].ToString();
        
        await this.LoadProvidersAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await this.LoadProvidersAsync();

        if (this.Input.Button == "cancel")
        {
            return this.LocalRedirect("/");
        }

        // the user clicked the "cancel" button
        if (Input.Button == "forgotpassword")
        {
            return this.RedirectToPage("/Account/ForgotPassword/Index", new
            {
                returnUrl = this.Input.ReturnUrl,
                clientId = this.Input.ClientId
            });
        }

        var result = await this._mediator.Send(new SecurityServiceCommands.LoginCommand(this.Input.Username, this.Input.Password, this.Input.RememberLogin));
        if (result.IsSuccess)
        {
            return this.LocalRedirect(string.IsNullOrWhiteSpace(this.Input.ReturnUrl) ? "/" : this.Input.ReturnUrl);
        }

        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message ?? "Invalid username or password.");
        return this.Page();
    }

    private async Task LoadProvidersAsync()
    {
        var result = await this._mediator.Send(new SecurityServiceQueries.GetExternalProvidersQuery());
        this.Providers = result.IsSuccess
            ? result.Data.Select(p => new ExternalProviderViewModel(p.Name, p.DisplayName)).ToArray()
            : Array.Empty<ExternalProviderViewModel>();
    }

    public sealed class InputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; } = "/";
        public string Button { get; set; } = "login";

        public string ClientId { get; set; }
    }

    public sealed record ExternalProviderViewModel(string Name, string DisplayName);
}

