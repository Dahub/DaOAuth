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
        private const string ACCESS_TOKEN_NAME = "access_token";
        private const string REFRESH_TOKEN_NAME = "refresh_token";

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

            if (String.IsNullOrEmpty(client_id))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_request", "Le paramètre client_id est requis", state));

            // si l'utilisateur n'est pas connecté, il faut l'inviter à le faire
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("LoginAuthorize", "User", new
                {
                    response_type = response_type,
                    client_id = client_id,
                    state = state,
                    redirect_uri = redirect_uri
                });

            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            if (!cs.IsClientValidForAuthorization(client_id, redirect_uri, response_type))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", state));

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
            string userName = ((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.NameIdentifier);

            if (!cs.IsClientAuthorizeByUser(client_id, userName))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "access_denied", "L'utilisateur a refusé l'accès au client", state));

            switch (response_type)
            {
                case "code":                    
                    return RedirectForResponseTypeCode(client_id, state, redirect_uri, cs, userName);
                case "token": // implicit grant pour spa applications
                    return RedirectForResponseTypeToken(client_id, state, redirect_uri, cs, userName);
                default:
                    return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unsupported_response_type", "La valeur du paramètre response_type doit être code", state));
            }
        }     

        [HttpPost]
        [Route("/token")]
        public JsonResult Token(TokenModel model)
        {
            if (String.IsNullOrEmpty(model.grant_type))
            {
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre grant_type doit être présent une et une seule fois et avoir une valeur");
            }

            switch (model.grant_type)
            {
                case "authorization_code":
                    return GenerateTokenForAuthorizationCodeGrant(model);
                case "refresh_token":
                    return GenerateTokenForRefreshToken(model);
                default:
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "unsupported_grant_type", "grant_type non pris en charge");
            }
        }

        [HttpPost]
        [Route("/introspect")]
        public JsonResult Introspect(string token)
        {
            var s = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError, s))
                return toReturnIfError;

            if (String.IsNullOrEmpty(token))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "le token est obligatoire");

            // décryptage du token
            var ticket = AuthConfig.OAuthBearerOptions.AccessTokenFormat.Unprotect(token);

            var currentUtc = new Microsoft.Owin.Infrastructure.SystemClock().UtcNow;
            if (ticket == null
                || ticket.Properties.ExpiresUtc < currentUtc
                || !ticket.Properties.Dictionary.ContainsKey("token_name")
                || ticket.Properties.Dictionary["token_name"] != ACCESS_TOKEN_NAME)
            {
                return Json(new
                {
                    active = false
                });
            }

            var clientId = ticket.Identity.FindFirstValue("ClientId");
            var userName = ticket.Identity.FindFirstValue(ClaimTypes.NameIdentifier);

            return Json(new
            {
                active = true,
                client_id = clientId,
                username = userName,
                exp = ticket.Properties.ExpiresUtc.Value.ToUnixTimeSeconds()
            });
        }

        private ActionResult RedirectForResponseTypeToken(string clientId, string state, string redirectUri, ClientService cs, string userName)
        {
            string location = String.Empty;
            var myToken = GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, clientId);
            location = String.Concat(redirectUri, "?token=", myToken, "?token_type=bearer?expires_in", ACCESS_TOKEN_LIFETIME * 60);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(location);
        }

        private ActionResult RedirectForResponseTypeCode(string clientId, string state, string redirectUri, ClientService cs, string userName)
        {
            string location = String.Empty;
            var myCode = cs.GenerateAndAddCodeToClient(clientId, userName);
            location = String.Concat(redirectUri, "?code=", myCode.CodeValue);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(location);
        }

        private JsonResult GenerateTokenForRefreshToken(TokenModel model)
        {
            try
            {
                var s = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };

                JsonResult toReturnIfError;
                if (!CheckAuthorizationHeader(out toReturnIfError, s))
                    return toReturnIfError;

                if (String.IsNullOrEmpty(model.refresh_token))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "refresh_token", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");

                // analyse du refresh token
                var ticket = AuthConfig.OAuthBearerOptions.AccessTokenFormat.Unprotect(model.refresh_token);

                var currentUtc = new Microsoft.Owin.Infrastructure.SystemClock().UtcNow;
                if (ticket == null 
                    || ticket.Properties.ExpiresUtc < currentUtc
                    || !ticket.Properties.Dictionary.ContainsKey("token_name")
                    || ticket.Properties.Dictionary["token_name"] != REFRESH_TOKEN_NAME)
                {
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");                    
                }
                
                var userName = ticket.Identity.FindFirstValue(ClaimTypes.NameIdentifier);
                var clientId = ticket.Identity.FindFirstValue("ClientId");

                // vérifier que le token n'ai pas été révoqué
                if(!s.IsRefreshTokenValid(userName, clientId, model.refresh_token))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");

                // générer un nouveau token (et refresh token à mettre à jour)
                model.client_id = clientId;

                return GenerateAccesTokenAndUpdateRefreshToken(model, s, userName);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(ex);
            }
        }

        private JsonResult GenerateTokenForAuthorizationCodeGrant(TokenModel model)
        {
            try
            {
                var s = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };

                JsonResult toReturnIfError;
                if (!CheckAuthorizationHeader(out toReturnIfError, s))
                    return toReturnIfError;

                if (String.IsNullOrEmpty(model.code))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");

                if (String.IsNullOrEmpty(model.redirect_uri))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre redirect_uri doit être présent une et une seule fois et avoir une valeur");

                if (String.IsNullOrEmpty(model.client_id))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur");

                if (!s.IsClientValidForAuthorization(model.client_id, model.redirect_uri, "code"))
                    return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_client", "client non valide");

                if (!s.IsCodeValidForAuthorizationCodeGrant(model.client_id, model.code))
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "code incorrect");

                string userName = s.ExtractUserNameFromCode(model.code);

                return GenerateAccesTokenAndUpdateRefreshToken(model, s, userName);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(ex);
            }
        }

        private JsonResult GenerateAccesTokenAndUpdateRefreshToken(TokenModel model, ClientService s, string userName)
        {
            string refreshToken = GenerateToken(REFRESH_TOKEN_LIFETIME, REFRESH_TOKEN_NAME, userName, model.client_id);
            s.UpdateRefreshTokenForClient(refreshToken, model.client_id, userName);

            return Json(new
            {
                access_token = GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, model.client_id),
                token_type = "bearer",
                expires_in = ACCESS_TOKEN_LIFETIME * 60, // en secondes
                refresh_token = refreshToken
            });
        }

        private bool CheckAuthorizationHeader(out JsonResult result, ClientService service)
        {
            result = null;

            if (Request.Headers["Authorization"] == null)
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }

            string[] authsInfos = Request.Headers["Authorization"].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (authsInfos.Length != 2)
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }

            if (!authsInfos[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }

            if (!service.AreClientCredentialsValid(authsInfos[1]))
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }            

            return true;
        }

        private string GenerateToken(int minutesLifeTime, string tokenName, string userName, string clientId)
        {
            ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim("Server","DaOAuth"),
                new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim("ClientId" , clientId)
            }, OAuthDefaults.AuthenticationType);

            AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
            var currentUtc = new Microsoft.Owin.Infrastructure.SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.Dictionary.Add("token_name", tokenName);
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

        private JsonResult GenerateErrorResponse(HttpStatusCode statusCode, string errorName, string errorDescription, string stateInfo)
        {
            Response.StatusCode = (int)statusCode;          

            if (String.IsNullOrEmpty(stateInfo))
            {
                var myError = new
                {
                    error = errorName,
                    error_description = errorDescription
                };
                return Json(myError);
            }
            else
            {
                var myError = new
                {
                    error = errorName,
                    error_description = errorDescription,
                    state = stateInfo
                };
                return Json(myError);
            }
        }

        private JsonResult GenerateErrorResponse(HttpStatusCode statusCode, string errorName, string errorDescription)
        {
            return GenerateErrorResponse(statusCode, errorName, errorDescription, String.Empty);
        }

        private bool IsUriCorrect(string uri)
        {
            Uri u = null;
            return Uri.TryCreate(uri, UriKind.Absolute, out u);
        }
    }
}