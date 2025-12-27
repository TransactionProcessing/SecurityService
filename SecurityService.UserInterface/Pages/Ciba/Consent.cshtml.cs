using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityServerHost.Pages.Ciba;

using Azure.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
[SecurityHeadersAttribute]
public class Consent : PageModel
{
    private readonly IBackchannelAuthenticationInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Index> _logger;

    public Consent(
        IBackchannelAuthenticationInteractionService interaction,
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

    public async Task<IActionResult> OnGet(string id)
    {
        View = await BuildViewModelAsync(id);
        if (View == null)
        {
            return RedirectToPage("/Home/Error/Index");
        }

        Input = new InputModel
        {
            Id = id
        };

        return Page();
    }

    private async Task<CompleteBackchannelLoginRequest> HandleNoButtonClicked(BackchannelUserLoginRequest request) {
        CompleteBackchannelLoginRequest result = new(Input.Id);

        // emit event
        await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
        
        return result;
    }

    private async Task<CompleteBackchannelLoginRequest> HandleYesButtonClicked(BackchannelUserLoginRequest request) {
        // if the user consented to some scope, build the response model
        if (Input.ScopesConsented != null && Input.ScopesConsented.Any())
        {
            var scopes = Input.ScopesConsented;
            if (ConsentOptions.EnableOfflineAccess == false)
            {
                scopes = scopes.Where(x => x != Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess);
            }

            CompleteBackchannelLoginRequest result = new CompleteBackchannelLoginRequest(Input.Id)
            {
                ScopesValuesConsented = scopes.ToArray(),
                Description = Input.Description
            };

            // emit event
            await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, result.ScopesValuesConsented, false));
            return result;
        }

        this.ModelState.AddModelError("", ConsentOptions.MustChooseOneErrorMessage);
        return null;
    }

    public async Task<IActionResult> OnPost()
    {
        // validate return url is still valid
        BackchannelUserLoginRequest? request = await _interaction.GetLoginRequestByInternalIdAsync(Input.Id);
        if (request == null || request.Subject.GetSubjectId() != User.GetSubjectId()) {
            _logger.LogError("Invalid id {id}", Input.Id);
            return RedirectToPage("/Home/Error/Index");
        }

        CompleteBackchannelLoginRequest result = null;

        // user clicked 'no' - send back the standard 'access_denied' response
        if (Input?.Button == "no") {
            result = await HandleNoButtonClicked(request);
        }
        // user clicked 'yes' - validate the data
        else if (Input?.Button == "yes") {
            result = await HandleYesButtonClicked(request);
        }
        else {
            ModelState.AddModelError("", ConsentOptions.InvalidSelectionErrorMessage);
        }

        if (result != null) {
            // communicate outcome of consent back to identityserver
            await _interaction.CompleteLoginRequestAsync(result);
            return RedirectToPage("/Ciba/All");
        }

        // we need to redisplay the consent UI
        View = await BuildViewModelAsync(Input.Id, Input);
        return Page();
    }

    private async Task<ViewModel> BuildViewModelAsync(string id, InputModel model = null)
    {
        var request = await _interaction.GetLoginRequestByInternalIdAsync(id);
        if (request != null && request.Subject.GetSubjectId() == User.GetSubjectId())
        {
            return CreateConsentViewModel(model, request);
        }
        else
        {
            _logger.LogError("No backchannel login request matching id: {id}", id);
        }
        return null;
    }

    private ViewModel CreateConsentViewModel(
        InputModel model,
        BackchannelUserLoginRequest request)
    {
        var vm = CreateBaseViewModel(request);

        vm.IdentityScopes = CreateIdentityScopes(model, request);
        vm.ApiScopes = CreateApiScopes(model, request);

        return vm;
    }

    private ViewModel CreateBaseViewModel(BackchannelUserLoginRequest request) =>
        new()
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            BindingMessage = request.BindingMessage
        };

    private ScopeViewModel[] CreateIdentityScopes(InputModel model,
                                                  BackchannelUserLoginRequest request)
    {
        return request.ValidatedResources.Resources.IdentityResources
            .Select(resource =>
                CreateScopeViewModel(
                    resource,
                    IsConsented(model, resource.Name)))
            .ToArray();
    }

    private List<ScopeViewModel> CreateApiScopes(InputModel model,
                                                 BackchannelUserLoginRequest request)
    {
        var apiScopes = request.ValidatedResources.ParsedScopes
            .Select(parsed => CreateApiScope(parsed, model, request))
            .Where(scope => scope != null)
            .ToList()!;

        AddOfflineAccessIfNeeded(apiScopes, model, request);

        return apiScopes;
    }

    private ScopeViewModel? CreateApiScope(ParsedScopeValue parsedScope,
                                           InputModel model,
                                           BackchannelUserLoginRequest request)
    {
        var apiScope = request.ValidatedResources.Resources
            .FindApiScope(parsedScope.ParsedName);

        if (apiScope == null)
            return null;

        var scopeVm = CreateScopeViewModel(
            parsedScope,
            apiScope,
            IsConsented(model, parsedScope.RawValue));

        scopeVm.Resources = CreateApiResources(parsedScope, request);

        return scopeVm;
    }

    private ResourceViewModel[] CreateApiResources(ParsedScopeValue parsedScope,
                                                   BackchannelUserLoginRequest request)
    {
        var resourceIndicators = request.RequestedResourceIndicators ?? Enumerable.Empty<string>();

        return request.ValidatedResources.Resources.ApiResources
            .Where(r => resourceIndicators.Contains(r.Name))
            .Where(r => r.Scopes.Contains(parsedScope.ParsedName))
            .Select(r => new ResourceViewModel
            {
                Name = r.Name,
                DisplayName = r.DisplayName ?? r.Name
            })
            .ToArray();
    }

    private void AddOfflineAccessIfNeeded(List<ScopeViewModel> apiScopes,
                                          InputModel model,
                                          BackchannelUserLoginRequest request)
    {
        if (!ConsentOptions.EnableOfflineAccess ||
            !request.ValidatedResources.Resources.OfflineAccess)
        {
            return;
        }

        apiScopes.Add(GetOfflineAccessScope(IsConsented(model, Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess)));
    }

    private static bool IsConsented(InputModel model, string scopeName) =>
        model?.ScopesConsented == null || model.ScopesConsented.Contains(scopeName);


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