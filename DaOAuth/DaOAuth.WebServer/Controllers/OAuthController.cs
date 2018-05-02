using DaOAuth.Dal.EF;
using DaOAuth.Service;
using DaOAuth.WebServer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    [HandleError]
    public class OAuthController : Controller
    {
        private const int ACCESS_TOKEN_LIFETIME = 10; // temps en minutes
        private const int REFRESH_TOKEN_LIFETIME = 5256000; // 10 ans

        [AllowAnonymous]
        [Route("/authorize")]
        public ActionResult Authorize(string response_type, string client_id, string state, string redirect_uri)
        {
            if (String.IsNullOrEmpty(redirect_uri))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "le paramètre redirect_uri doit être renseigné");

            if (!IsUriCorrect(redirect_uri))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "l'url de redirection est incorrecte");

            if (String.IsNullOrEmpty(response_type))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_request", "Le paramètre response_type est requis", state));

            if (response_type != "code")
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unsupported_response_type", "La valeur du paramètre response_type doit être code", state));

            if (String.IsNullOrEmpty(client_id))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_request", "Le paramètre client_id est requis", state));

            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            if (!cs.IsClientValidForAuthorizationCodeGrant(client_id, redirect_uri))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", state));

            // si l'utilisateur n'est pas connecté, il faut l'inviter à le faire
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("LoginAuthorize", "User", new
                {
                    response_type = response_type,
                    client_id = client_id,
                    state = state,
                    redirect_uri = redirect_uri
                });

            // vérifier que l'utilisateur a bien connaissance du client, sinon, prompt d'autorisation
            if (!cs.HasUserAuthorizeOrDeniedClientAccess(client_id, ((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.NameIdentifier)))
                return RedirectToAction("AuthorizeClient", "User", new
                {
                    response_type = response_type,
                    client_id = client_id,
                    state = state,
                    redirect_uri = redirect_uri
                });

            // l'utilisateur a t'il authorisé le client ?
            if (!cs.IsClientAuthorizeByUser(client_id, ((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.NameIdentifier)))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "access_denied", "L'utilisateur a refusé l'accès au client", state));

            // génération d'un code 
            string location = String.Empty;
            var myCode = cs.GenerateAndAddCodeToClient(client_id);
            location = String.Concat(redirect_uri, "?code=", myCode.CodeValue);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);

            return Redirect(location);
        }

        [HttpPost]
        [Route("/token")]
        public JsonResult Token(TokenModel model)
        {
            if (String.IsNullOrEmpty(model.grant_type))
            {
                return GenerateErrorResponse("invalid_request", "Le paramètre grant_type doit être présent une et une seule fois et avoir une valeur");
            }

            switch (model.grant_type)
            {
                case "authorization_code":
                    return GenerateTokenForAuthorizationCodeGrant(model);
                default:
                    return GenerateErrorResponse("unsupported_grant_type", "grant_type non pris en charge");
            }
        }

        private JsonResult GenerateTokenForAuthorizationCodeGrant(TokenModel model)
        {
            try
            {
                if (String.IsNullOrEmpty(model.code))
                {
                    return GenerateErrorResponse("invalid_request", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");
                }

                if (String.IsNullOrEmpty(model.redirect_uri))
                {
                    return GenerateErrorResponse("invalid_request", "Le paramètre redirect_uri doit être présent une et une seule fois et avoir une valeur");
                }

                if (String.IsNullOrEmpty(model.client_id))
                {
                    return GenerateErrorResponse("invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur");
                }

                var s = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };

                // vérification du paramètre d'authentification (basic)
                if (Request.Headers["Authorization"] == null)
                    return GenerateErrorResponse("unauthorized_client", "L'authentification du client a échoué");

                string[] authsInfos = Request.Headers["Authorization"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if(authsInfos.Length != 2)
                    return GenerateErrorResponse("unauthorized_client", "L'authentification du client a échoué");

                if(!authsInfos[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
                    return GenerateErrorResponse("unauthorized_client", "L'authentification du client a échoué");

                if(!s.AreClientCredentialsValid(authsInfos[1]))
                    return GenerateErrorResponse("unauthorized_client", "L'authentification du client a échoué");

                if (!s.IsClientValidForAuthorizationCodeGrant(model.client_id, model.redirect_uri))
                {
                    return GenerateErrorResponse("invalid_client", "client non valide");
                }

                if (!s.IsCodeValidForAuthorizationCodeGrant(model.client_id, model.code))
                {
                    return GenerateErrorResponse("invalid_grant", "code incorrect");
                }

                string refreshToken = GenerateToken(REFRESH_TOKEN_LIFETIME);

                s.UpdateRefreshTokenForClient(refreshToken, model.client_id);

                return Json(new
                {
                    access_token = GenerateToken(ACCESS_TOKEN_LIFETIME),
                    token_type = "bearer",
                    expires_in = ACCESS_TOKEN_LIFETIME * 60, // en secondes
                    refresh_token = refreshToken
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(ex);
            }
        }

        private string GenerateToken(int minutesLifeTime)
        {
            ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim("Server","DaOAuth")
            }, OAuthDefaults.AuthenticationType);

            AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
            var currentUtc = new Microsoft.Owin.Infrastructure.SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromMinutes(minutesLifeTime));
            return AuthConfig.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
        }

        private string GenerateRedirectErrorMessage(string redirectUri, string errorName, string errorDescription, string stateInfo)
        {
            if (String.IsNullOrEmpty(stateInfo))
                return GenerateRedirectErrorMessage(redirectUri, errorName, errorDescription);

            return String.Format("{0}?error={1}&error_description={2}&state={3}", redirectUri, errorName, errorDescription, stateInfo);
        }

        private string GenerateRedirectErrorMessage(string redirectUri, string errorName, string errorDescription)
        {
            return String.Format("{0}?error={1}&error_description={2}", redirectUri, errorName, errorDescription);
        }

        private JsonResult GenerateErrorResponse(string errorName, string errorDescription, string stateInfo)
        {
            if (String.IsNullOrEmpty(stateInfo))
                return GenerateErrorResponse(errorName, errorDescription);

            var myError = new
            {
                error = errorName,
                error_description = errorDescription,
                state = stateInfo
            };

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(myError);
        }

        private JsonResult GenerateErrorResponse(string errorName, string errorDescription)
        {
            var myError = new
            {
                error = errorName,
                error_description = errorDescription
            };

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json(myError);
        }

        private bool IsUriCorrect(string uri)
        {
            Uri u = null;
            return Uri.TryCreate(uri, UriKind.Absolute, out u);
        }
    }
}