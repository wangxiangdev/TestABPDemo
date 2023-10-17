using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.OpenIddict.ViewModels.Authorization;
using Microsoft.AspNetCore;
using System.Linq;
using Volo.Abp.OpenIddict;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Authentication.Cookies;
using Volo.Abp.OpenIddict.Controllers;
using Serilog;
using Volo.Abp.DependencyInjection;

namespace TestDemo.Controllers
{
    [Route("connect/authorize")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(Volo.Abp.OpenIddict.Controllers.AuthorizeController))]
    public class AuthorizeController : Volo.Abp.OpenIddict.Controllers.AuthorizeController
    {
        /*
         * Volo.Abp.OpenIddict.Controllers
         * Volo.Abp.OpenIddict.Controllers.AbpOpenIdDictControllerBase
         * Volo.Abp.OpenIddict.Controllers.AuthorizeController
         * Volo.Abp.OpenIddict.Controllers.LogoutController
         * Volo.Abp.OpenIddict.Controllers.TokenController
         * Volo.Abp.OpenIddict.Controllers.UserInfoController
         * Volo.Abp.Account.AccountController
         */
        
        [HttpGet, HttpPost]
        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> HandleAsync()
        {
            Log.Information("██████This is a Custom Authorization !!!!!!!!");
            var request = await GetOpenIddictServerRequestAsync(HttpContext);

            // If prompt=login was specified by the client application,
            // immediately return the user agent to the login page.
            if (request.HasPrompt(Prompts.Login))
            {
                // To avoid endless login -> authorization redirects, the prompt=login flag
                // is removed from the authorization request payload before redirecting the user.
                var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

                var parameters = Request.HasFormContentType ?
                    Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                    Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    //authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme, 
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                    });
            }

            // Retrieve the user principal stored in the authentication cookie.
            // If a max_age parameter was provided, ensure that the cookie is not too old.
            // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (result == null || !result.Succeeded || (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in.
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }!));
                }

                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    //authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            // Retrieve the profile of the logged in user.
            var user = await UserManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException(L["TheUserDetailsCannotBbeRetrieved"]);

            // Retrieve the application details from the database.
            var application = await ApplicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException(L["DetailsConcerningTheCallingClientApplicationCannotBeFound"]);

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await AuthorizationManager.FindAsync(
                subject: await UserManager.GetUserIdAsync(user),
                client: (await ApplicationManager.GetIdAsync(application))!,
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            switch (await ApplicationManager.GetConsentTypeAsync(application))
            {
                // If the consent is external (e.g when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database.
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                        }!));

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                    var principal = await SignInManager.CreateUserPrincipalAsync(user);

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    principal.SetScopes(request.GetScopes());
                    principal.SetResources(await ScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    if (authorization == null)
                    {
                        authorization = await AuthorizationManager.CreateAsync(
                            principal: principal,
                            subject: await UserManager.GetUserIdAsync(user),
                            client: (await ApplicationManager.GetIdAsync(application))!,
                            type: AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes());
                    }

                    principal.SetAuthorizationId(await AuthorizationManager.GetIdAsync(authorization));

                    await SetClaimsDestinationsAsync(principal);

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                        }!));

                // In every other case, render the consent form.
                default:
                    return View("Authorize", new AuthorizeViewModel
                    {
                        ApplicationName = await ApplicationManager.GetDisplayNameAsync(application),
                        Scope = request.Scope
                    });
            }
        }

        [HttpPost]
        [Authorize]
        [Route("callback")]
        public override async Task<IActionResult> HandleCallbackAsync()
        {
            if (await HasFormValueAsync("deny"))
            {
                // Notify OpenIddict that the authorization grant has been denied by the resource owner
                // to redirect the user agent to the client application using the appropriate response_mode.
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var request = await GetOpenIddictServerRequestAsync(HttpContext);

            // Retrieve the profile of the logged in user.
            var user = await UserManager.GetUserAsync(User) ??
                       throw new InvalidOperationException(L["TheUserDetailsCannotBbeRetrieved"]);

            // Retrieve the application details from the database.
            var application = await ApplicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException(L["DetailsConcerningTheCallingClientApplicationCannotBeFound"]);

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await AuthorizationManager.FindAsync(
                subject: await UserManager.GetUserIdAsync(user),
                client: (await ApplicationManager.GetIdAsync(application))!,
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            // Note: the same check is already made in the other action but is repeated
            // here to ensure a malicious user can't abuse this POST-only endpoint and
            // force it to return a valid response without the external authorization.
            if (!authorizations.Any() && await ApplicationManager.HasConsentTypeAsync(application, ConsentTypes.External))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application."
                    }!));
            }

            var principal = await SignInManager.CreateUserPrincipalAsync(user);

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await ScopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes.
            var authorization = authorizations.LastOrDefault();
            if (authorization == null)
            {
                authorization = await AuthorizationManager.CreateAsync(
                    principal: principal,
                    subject: await UserManager.GetUserIdAsync(user),
                    client: (await ApplicationManager.GetIdAsync(application))!,
                    type: AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());
            }

            principal.SetAuthorizationId(await AuthorizationManager.GetIdAsync(authorization));
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await GetResourcesAsync(request.GetScopes()));

            await SetClaimsDestinationsAsync(principal);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /*
        [Authorize]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            throw new NotImplementedException();
        }

        [HttpGet("~/connect/logout")]
        public async Task<IActionResult> Logout()
        {
            throw new NotImplementedException();
        }


        [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            throw new NotImplementedException();

        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                case Claims.Name:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }
        */
    }
}
