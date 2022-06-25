// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace IdentityServerHost.Pages.EmailConfirmation;

using System;
using System.Collections.Generic;
using System.Linq;

public class ViewModel
{
    public string Username { get; set; }

    public string Token { get; set; }

    public String UserMessage { get; set; }
}