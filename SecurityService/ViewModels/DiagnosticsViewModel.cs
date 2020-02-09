// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SecurityService.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using IdentityModel;
    using Microsoft.AspNetCore.Authentication;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(AuthenticateResult result)
        {
            this.AuthenticateResult = result;

            if (result.Properties.Items.ContainsKey("client_list"))
            {
                String encoded = result.Properties.Items["client_list"];
                Byte[] bytes = Base64Url.Decode(encoded);
                String value = Encoding.UTF8.GetString(bytes);

                this.Clients = JsonConvert.DeserializeObject<string[]>(value);
            }
        }

        public AuthenticateResult AuthenticateResult { get; }
        public IEnumerable<string> Clients { get; } = new List<string>();
    }
}