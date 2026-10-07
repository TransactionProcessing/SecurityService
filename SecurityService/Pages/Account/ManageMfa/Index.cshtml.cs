using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.Database.Entities;

namespace SecurityService.Pages.Account.ManageMfa;

[Authorize]
public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MfaAccountService _mfaAccountService;

    public IndexModel(UserManager<ApplicationUser> userManager, MfaAccountService mfaAccountService)
    {
        _userManager = userManager;
        _mfaAccountService = mfaAccountService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public MfaEnrollment? Enrollment { get; private set; }

    public bool IsMfaEnabled { get; private set; }

    public string QrCodeDataUri { get; private set; } = string.Empty;

    public string StatusMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await this.LoadStateAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostBeginEnrollmentAsync(CancellationToken cancellationToken)
    {
        var user = await this.GetUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        this.Enrollment = await _mfaAccountService.BeginEnrollmentAsync(user, "SecurityService", cancellationToken);
        this.QrCodeDataUri = CreateQrCodeDataUri(this.Enrollment.OtpauthUri);
        await this.LoadStateAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmEnrollmentAsync(CancellationToken cancellationToken)
    {
        var user = await this.GetUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var confirmed = await _mfaAccountService.ConfirmEnrollmentAsync(user, Input.Code, cancellationToken);
        StatusMessage = confirmed ? "MFA has been enabled." : "The authenticator code was invalid.";
        await this.LoadStateAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(CancellationToken cancellationToken)
    {
        var user = await this.GetUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var removed = await _mfaAccountService.RemoveAuthenticatorAsync(user, Input.CurrentPassword, Input.Code, cancellationToken);
        StatusMessage = removed ? "MFA has been removed." : "The password or authenticator code was invalid.";
        await this.LoadStateAsync(cancellationToken);
        return Page();
    }

    private async Task<ApplicationUser?> GetUserAsync() => await _userManager.GetUserAsync(this.User);

    private async Task LoadStateAsync(CancellationToken _)
    {
        var user = await this.GetUserAsync();
        this.IsMfaEnabled = user is not null && await _userManager.GetTwoFactorEnabledAsync(user);
    }

    private static string CreateQrCodeDataUri(string value)
    {
        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(qrCodeData).GetGraphic(12);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    public sealed class InputModel
    {
        public string Code { get; set; } = string.Empty;

        public string CurrentPassword { get; set; } = string.Empty;
    }
}
