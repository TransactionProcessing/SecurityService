using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityService.BusinessLogic.Mfa;
using SecurityService.Database.Entities;

namespace SecurityService.Pages.Account.ManageMfa;

[Authorize]
public sealed class RecoveryCodesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MfaAccountService _mfaAccountService;

    public RecoveryCodesModel(UserManager<ApplicationUser> userManager, MfaAccountService mfaAccountService)
    {
        _userManager = userManager;
        _mfaAccountService = mfaAccountService;
    }

    public IReadOnlyList<string> RecoveryCodes { get; private set; } = Array.Empty<string>();

    public string StatusMessage { get; private set; } = string.Empty;

    public async Task<IActionResult> OnPostGenerateAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return Challenge();
        }

        RecoveryCodes = await _mfaAccountService.GenerateRecoveryCodesAsync(user, 10, cancellationToken);
        StatusMessage = "Store these recovery codes securely. They will not be shown again.";
        return Page();
    }

    public async Task<IActionResult> OnPostRevokeAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(this.User);
        if (user is null)
        {
            return Challenge();
        }

        await _mfaAccountService.RevokeRecoveryCodesAsync(user, cancellationToken);
        StatusMessage = "Recovery codes have been revoked.";
        return Page();
    }
}
