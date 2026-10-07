using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.Database.Entities;

namespace SecurityService.Pages.Account.EnrollMfa;

[AllowAnonymous]
public sealed class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly MfaAccountService _mfaAccountService;
    private readonly MfaEnrollmentTransactionProtector _transactionProtector;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        MfaAccountService mfaAccountService,
        MfaEnrollmentTransactionProtector transactionProtector)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mfaAccountService = mfaAccountService;
        _transactionProtector = transactionProtector;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public MfaEnrollment? Enrollment { get; private set; }

    public string QrCodeDataUri { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string transaction, CancellationToken cancellationToken)
    {
        this.Input.Transaction = transaction;
        return await this.LoadEnrollmentAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (_transactionProtector.TryUnprotect(this.Input.Transaction, out var transaction) == false || transaction is null)
        {
            ModelState.AddModelError(string.Empty, "The MFA enrollment request has expired. Start signing in again.");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(transaction.UserId);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The MFA enrollment request is no longer valid.");
            return Page();
        }

        var confirmed = await _mfaAccountService.ConfirmEnrollmentAsync(user, this.Input.Code, cancellationToken);
        if (confirmed == false)
        {
            ModelState.AddModelError(string.Empty, "The authenticator code was invalid.");
            await this.LoadEnrollmentAsync(cancellationToken);
            return Page();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return this.LocalRedirect(string.IsNullOrWhiteSpace(transaction.ReturnUrl) ? "/" : transaction.ReturnUrl);
    }

    private async Task<IActionResult> LoadEnrollmentAsync(CancellationToken cancellationToken)
    {
        if (_transactionProtector.TryUnprotect(this.Input.Transaction, out var transaction) == false || transaction is null)
        {
            ModelState.AddModelError(string.Empty, "The MFA enrollment request has expired. Start signing in again.");
            return Page();
        }

        var user = await _userManager.FindByIdAsync(transaction.UserId);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "The MFA enrollment request is no longer valid.");
            return Page();
        }

        this.Enrollment = await _mfaAccountService.GetEnrollmentAsync(user, "SecurityService", cancellationToken);
        this.QrCodeDataUri = CreateQrCodeDataUri(this.Enrollment.OtpauthUri);
        return Page();
    }

    private static string CreateQrCodeDataUri(string value)
    {
        using var generator = new QRCoder.QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(value, QRCoder.QRCodeGenerator.ECCLevel.Q);
        var png = new QRCoder.PngByteQRCode(qrCodeData).GetGraphic(12);
        return $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    public sealed class InputModel
    {
        public string Transaction { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;
    }
}
