using Microsoft.AspNetCore.Mvc;
using SAMLHost.Models;
using SAMLHost.Services;
using System.Net;

namespace SAMLHost.Controllers;
public class HomeController : Controller
{
    public const string SamlResponse = "SAMLResponse";

    private readonly SamlService _samlService;
    private readonly SamlIdentityProviderOptions _identityProvider;
    private readonly SamlServiceProviderOptions _serviceProvider;

    public HomeController(SamlService samlService, SamlIdentityProviderOptions identityProvider, SamlServiceProviderOptions serviceProvider)
    {
        _samlService = samlService;
        _identityProvider = identityProvider;
        _serviceProvider = serviceProvider;
    }

    [Route("LoginSSO")]
    public ActionResult LoginSSO()
    {
        return Redirect(string.Concat(_identityProvider.SingleSignOnServiceUrl, SamlParam(_identityProvider.SingleSignOnServiceUrl),
           WebUtility.UrlEncode(_samlService.GenerateRequest(SamlService.GenerateId(), SamlService.IssueInstant()))));
    }

    [HttpPost]
    [Route("Consume")]
    public ActionResult Consume()
    {
        var response = new XmlResponse(Request.Form[SamlResponse]);

        if (_samlService.ResponseIsValid(response))
        {
            var userId = response.GetSubject();
            if (userId == null)
            {
                if (!string.IsNullOrEmpty(_identityProvider.LogoutUrl))
                {
                    return Redirect(_identityProvider.LogoutUrl);
                }
                else
                {
                    return BadRequest();
                }
            }

            var token = _samlService.SetSsoToken(userId);
            if (token == null)
                return new ContentResult { Content = string.Concat(@"SSO failed. \n User ", userId, " is invalid.") };

            return Redirect(string.Concat(_serviceProvider.AuthenticatedRedirectUrl, "?SSOtoken=", token, "&SamlIssuer=", _identityProvider.EntityId));
        }
        return new ContentResult { Content = @"SSO failed. \n Certificate is invalid." };
    }

    private object SamlParam(string identityIssuerUrl)
    {
        return identityIssuerUrl.Contains("?") ? "&SAMLRequest=" : "?SAMLRequest=";
    }
}
