// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.ChangePassword;

public class IndexInputModel
{
    public string Username { get; set; }

    public string ReturnUrl { get; set; }

    [Required]
    public string CurrentPassword { get; set; }
    [Required]
    public string NewPassword { get; set; }

    [Required] 
    public string ConfirmPassword { get; set; }

    public string Button { get; set; }
    public string ClientId { get; set; }
}