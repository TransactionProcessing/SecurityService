using Duende.IdentityModel;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.Consent;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[Authorize]
[SecurityHeadersAttribute]
public class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Index> _logger;

    public Index(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Index> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    public ViewModel View { get; set; }
        
    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        View = await BuildViewModelAsync(returnUrl);
        if (View == null)
        {
            return RedirectToPage("/Home/Error/Index");
        }

        Input = new InputModel
        {
            ReturnUrl = returnUrl,
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var request = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);
        if (request == null)
            return RedirectToPage("/Home/Error/Index");

        var consent = await HandleConsentAsync(request);
        if (consent == null)
        {
            View = await BuildViewModelAsync(Input.ReturnUrl, Input);
            return Page();
        }

        await _interaction.GrantConsentAsync(request, consent);

        return request.IsNativeClient()
            ? this.LoadingPage(Input.ReturnUrl)
            : Redirect(Input.ReturnUrl);
    }

    private async Task<ConsentResponse?> HandleConsentAsync(AuthorizationRequest request)
    {
        return Input?.Button switch
        {
            "no" => await DenyConsentAsync(request),
            "yes" => await GrantConsentAsync(request),
            _ => InvalidSelection()
        };
    }

    private async Task<ConsentResponse> DenyConsentAsync(AuthorizationRequest request)
    {
        await _events.RaiseAsync(
            new ConsentDeniedEvent(
                User.GetSubjectId(),
                request.Client.ClientId,
                request.ValidatedResources.RawScopeValues));

        return new ConsentResponse
        {
            Error = AuthorizationError.AccessDenied
        };
    }

    private async Task<ConsentResponse?> GrantConsentAsync(AuthorizationRequest request)
    {
        if (Input.ScopesConsented == null || !Input.ScopesConsented.Any())
        {
            ModelState.AddModelError("", ConsentOptions.MustChooseOneErrorMessage);
            return null;
        }

        var scopes = FilterScopes(Input.ScopesConsented);

        var consent = new ConsentResponse
        {
            RememberConsent = Input.RememberConsent,
            ScopesValuesConsented = scopes,
            Description = Input.Description
        };

        await _events.RaiseAsync(
            new ConsentGrantedEvent(
                User.GetSubjectId(),
                request.Client.ClientId,
                request.ValidatedResources.RawScopeValues,
                consent.ScopesValuesConsented,
                consent.RememberConsent));

        return consent;
    }

    private string[] FilterScopes(IEnumerable<string> scopes)
    {
        if (!ConsentOptions.EnableOfflineAccess)
        {
            scopes = scopes.Where(x =>
                x != Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess);
        }

        return scopes.ToArray();
    }

    private ConsentResponse? InvalidSelection()
    {
        ModelState.AddModelError("", ConsentOptions.InvalidSelectionErrorMessage);
        return null;
    }

    
    private async Task<ViewModel> BuildViewModelAsync(string returnUrl, InputModel model = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (request != null)
        {
            return CreateConsentViewModel(model, returnUrl, request);
        }
        else
        {
            _logger.LogError("No consent request matching request: {0}", returnUrl);
        }
        return null;
    }

    private ViewModel CreateConsentViewModel(
        InputModel model, string returnUrl,
        AuthorizationRequest request)
    {
        var vm = new ViewModel
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };

        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources
            .Select(x => CreateScopeViewModel(x, model?.ScopesConsented == null || model.ScopesConsented?.Contains(x.Name) == true))
            .ToArray();

        var resourceIndicators = request.Parameters.GetValues(OidcConstants.AuthorizeRequest.Resource) ?? Enumerable.Empty<string>();
        var apiResources = request.ValidatedResources.Resources.ApiResources.Where(x => resourceIndicators.Contains(x.Name));

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, model == null || model.ScopesConsented?.Contains(parsedScope.RawValue) == true);
                scopeVm.Resources = apiResources.Where(x => x.Scopes.Contains(parsedScope.ParsedName))
                    .Select(x => new ResourceViewModel
                    {
                        Name = x.Name,
                        DisplayName = x.DisplayName ?? x.Name,
                    }).ToArray();
                apiScopes.Add(scopeVm);
            }
        }
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(model == null || model.ScopesConsented?.Contains(Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess) == true));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Name = identity.Name,
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!String.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new ScopeViewModel
        {
            Name = parsedScopeValue.ParsedName,
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    private ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}