// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServerHost.Pages.ForgotPassword;

public class IndexInputModel
{
    [Required]
    public string Username { get; set; }

    public string ReturnUrl { get; set; }

    [Required]
    public string EmailAddress { get; set; }

    public string Button { get; set; }
    public string ClientId { get; set; }
}