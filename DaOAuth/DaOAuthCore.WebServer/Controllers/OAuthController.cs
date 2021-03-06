﻿using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;

namespace DaOAuthCore.WebServer.Controllers
{
    public class OAuthController : Controller
    {
        private IClientService _clientService;
        private IUserService _userService;
        private AppConfiguration _configuration;
        private IJwtService _jwtService;

        private const int ACCESS_TOKEN_LIFETIME = 10; // temps en minutes
        private const int REFRESH_TOKEN_LIFETIME = 10 * 365 * 24 * 60; // 5256000 => 10 ans exprimés en minutes
        private const string ACCESS_TOKEN_NAME = "access_token";
        private const string REFRESH_TOKEN_NAME = "refresh_token";

        public OAuthController([FromServices] IClientService cs, [FromServices] IUserService us, [FromServices] IJwtService js, IOptions<AppConfiguration> conf)
        {
            _clientService = cs;
            _userService = us;
            _jwtService = js;
            _configuration = conf.Value;            
        }

        [AllowAnonymous]
        [Route("/authorize")]
        public ActionResult Authorize([FromQuery(Name = "response_type")] string responseType,
            [FromQuery(Name = "client_id")] string clientId,
            [FromQuery(Name = "state")] string state,
            [FromQuery(Name = "redirect_uri")] string redirectUri,
            [FromQuery(Name = "scope")] string scope)
        {
            if (String.IsNullOrEmpty(redirectUri))
                return StatusCode((int)HttpStatusCode.BadRequest, "le paramètre redirect_uri doit être renseigné");

            if (!IsUriCorrect(redirectUri))
                return StatusCode((int)HttpStatusCode.BadRequest, "l'url de redirection est incorrecte");

            if (String.IsNullOrEmpty(responseType))
                return Redirect( GenerateRedirectErrorMessage(redirectUri, "invalid_request", "Le paramètre response_type est requis", state));

            if (String.IsNullOrEmpty(clientId))
                return Redirect(GenerateRedirectErrorMessage(redirectUri, "invalid_request", "Le paramètre client_id est requis", state));

            if (!_clientService.IsClientValidForAuthorization(clientId, new Uri(redirectUri, UriKind.Absolute) , responseType))
                return Redirect(GenerateRedirectErrorMessage(redirectUri, "unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", state));

            // vérification des scopes proposés
            if (!_clientService.AreScopesAuthorizedForClient(clientId, scope))
                return Redirect(GenerateRedirectErrorMessage(redirectUri, "invalid_scope", "Scopes demandés invalides"));

            // si l'utilisateur n'est pas connecté, il faut l'inviter à le faire
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("LoginAuthorize", "User", new
                {
                    response_type = responseType,
                    client_id = clientId,
                    state = state,
                    redirect_uri = redirectUri,
                    scope = scope
                });

            // vérifier que l'utilisateur a bien connaissance du client, sinon, prompt d'autorisation
            if (!_clientService.HasUserAuthorizeOrDeniedClientAccess(clientId, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value))
                return RedirectToAction("AuthorizeClient", "User", new
                {
                    response_type = responseType,
                    client_id = clientId,
                    state = state,
                    redirect_uri = redirectUri,
                    scope = scope
                });

            // l'utilisateur a t'il authorisé le client ?
            string userName = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;
            
            if (!_clientService.IsClientAuthorizeByUser(clientId, userName, out Guid userPublicId))
                return Redirect(GenerateRedirectErrorMessage(redirectUri, "access_denied", "L'utilisateur a refusé l'accès au client", state));

            switch (responseType)
            {
                case "code":
                    return RedirectForResponseTypeCode(clientId, state, redirectUri, userName, scope, userPublicId);
                case "token": // implicit grant pour spa applications
                    return RedirectForResponseTypeToken(clientId, state, redirectUri, userName, scope, userPublicId);
                default:
                    return Redirect(GenerateRedirectErrorMessage(redirectUri, "unsupported_response_type", "La valeur du paramètre response_type doit être code", state));
            }
        }

        [HttpPost]
        [Route("/token")]
        public JsonResult Token([FromForm] TokenModel model)
        {
            try
            {
                if (model == null || String.IsNullOrEmpty(model.GrantType))
                {
                    return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre grant_type doit être présent une et une seule fois et avoir une valeur");
                }

                switch (model.GrantType)
                {
                    case "authorization_code":
                        return GenerateTokenForAuthorizationCodeGrant(model);
                    case "refresh_token":
                        return GenerateTokenForRefreshToken(model);
                    case "password":
                        return GenerateTokenForPasswordGrant(model);
                    case "client_credentials":
                        return GenerateTokenForClientCredentailsGrant(model);
                    default:
                        return GenerateErrorResponse(HttpStatusCode.BadRequest, "unsupported_grant_type", "grant_type non pris en charge");
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(ex);
            }
        }

        [HttpPost]
        [Route("/introspect")]
        public JsonResult Introspect([FromServices] IRessourceServerService rsService, [FromForm] IntrospectTokenModel model)
        {
            if (!CheckAuthorizationHeaderForRessourceServer(rsService))
            {
                return Json(new
                {
                    active = false
                });
            }

            if (String.IsNullOrEmpty(model.token))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "le token est obligatoire");

            if (!_jwtService.CheckIfTokenIsValid(model.token, ACCESS_TOKEN_NAME, out long expire, out ClaimsPrincipal user))
            {
                return Json(new
                {
                    active = false
                });
            }

            // on récupère les infos
            string scope = _jwtService.GetValueFromClaim(user.Claims, "scope");
            var clientId = _jwtService.GetValueFromClaim(user.Claims, "client_id");
            var name = _jwtService.GetValueFromClaim(user.Claims, ClaimTypes.NameIdentifier);
            var userPublicId = _jwtService.GetValueFromClaim(user.Claims, "user_public_id");
            string[] auds = rsService.GetAllRessourcesServersNames();
            return Json(new
            {
                active = true,
                exp = expire,
                aud = auds,
                client_id = clientId,
                name = name,
                scope = scope,                
                user_public_id = userPublicId                
            });
        }

        #region private        

        private ActionResult RedirectForResponseTypeToken(string clientId, string state, string redirectUri, string userName, string scope, Guid userPublicId)
        {          
            var myToken = _jwtService.GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, clientId, scope, userPublicId);
            string location = String.Concat(redirectUri, "?token=", myToken, "?token_type=bearer?expires_in", ACCESS_TOKEN_LIFETIME * 60);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(new Uri(location).AbsoluteUri);
        }

        private ActionResult RedirectForResponseTypeCode(string clientId, string state, string redirectUri, string userName, string scope, Guid userPublicId)
        {
            var myCode = _clientService.GenerateAndAddCodeToClient(clientId, userName, scope, userPublicId);
            string location = String.Concat(redirectUri, "?code=", myCode);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(new Uri(location).AbsoluteUri);
        }

        private JsonResult GenerateTokenForClientCredentailsGrant(TokenModel model)
        {
            if (!CheckAuthorizationHeader(out JsonResult toReturnIfError))
                return toReturnIfError;

            var clientId = _clientService.GetClientIdFromAuthorizationHeaderValue(Request.Headers["Authorization"]);

            if (!_clientService.AreScopesAuthorizedForClient(model.ClientId, model.Scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            return Json(new
            {
                access_token = _jwtService.GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, clientId, model.Scope, null),
                token_type = "bearer",
                expires_in = ACCESS_TOKEN_LIFETIME * 60
            });
        }

        private JsonResult GenerateTokenForRefreshToken(TokenModel model)
        {
            if (!CheckAuthorizationHeader(out JsonResult toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.RefreshToken))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "refresh_token", "Le paramètre refresh_token doit être présent une et une seule fois et avoir une valeur");

            // analyse du refresh token
            if (!_jwtService.CheckIfTokenIsValid(model.RefreshToken, REFRESH_TOKEN_NAME, out long expire, out ClaimsPrincipal user))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");

            // on récupère les infos
            string scope = _jwtService.GetValueFromClaim(user.Claims, "scope");
            var clientId = _jwtService.GetValueFromClaim(user.Claims, "client_id");
            var userName = _jwtService.GetValueFromClaim(user.Claims, ClaimTypes.NameIdentifier);
            Guid userPublicId = Guid.Parse(_jwtService.GetValueFromClaim(user.Claims, "user_public_id"));

            // vérifier que le token n'ai pas été révoqué
            if (!_clientService.IsRefreshTokenValid(userName, clientId, model.RefreshToken))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");

            // générer un nouveau token (et refresh token à mettre à jour)
            model.ClientId = clientId;

            if (!_clientService.AreScopesAuthorizedForClient(model.ClientId, scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            return GenerateAccesTokenAndUpdateRefreshToken(model, userName, scope, userPublicId);
        }

        private JsonResult GenerateTokenForAuthorizationCodeGrant(TokenModel model)
        {
            if (!CheckAuthorizationHeader(out JsonResult toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.Code))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");

            if (model.RedirectUrl == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre redirect_uri doit être présent une et une seule fois et avoir une valeur");

            if (String.IsNullOrEmpty(model.ClientId))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur");

            if (!_clientService.IsClientValidForAuthorization(model.ClientId, model.RedirectUrl, "code"))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_client", "client non valide");

            CodeDto codeInfos = _clientService.GetCodeInfos(model.ClientId, model.Code);

            if (!codeInfos.IsValid)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "code incorrect");

            return GenerateAccesTokenAndUpdateRefreshToken(model, codeInfos.UserName, codeInfos.Scope, codeInfos.UserPublicId);
        }

        private JsonResult GenerateTokenForPasswordGrant(TokenModel model)
        {
            /* RFC6749 4.3.2 : Since this access token request utilizes the resource owner's
            password, the authorization server MUST protect the endpoint against
            brute force attacks(e.g., using rate - limitation or generating
            alerts). */
            Thread.Sleep(50);

            if (!CheckAuthorizationHeader(out JsonResult toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.Username))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre username doit être présent une et une seule fois et avoir une valeur");

            if (String.IsNullOrEmpty(model.Password))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre password doit être présent une et une seule fois et avoir une valeur");

            // vérification de la validé des identifiants          
            if (_userService.Find(model.Username, model.Password) == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "username ou password incorrect");

            model.ClientId = _clientService.GetClientIdFromAuthorizationHeaderValue(Request.Headers["Authorization"]);

            if (!_clientService.AreScopesAuthorizedForClient(model.ClientId, model.Scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            Guid? userPublicId = _clientService.GetUserPublicId(model.ClientId, model.Username);

            if(!userPublicId.HasValue)
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");

            return GenerateAccesTokenAndUpdateRefreshToken(model, model.Username, model.Scope, userPublicId.Value);
        }

        private JsonResult GenerateAccesTokenAndUpdateRefreshToken(TokenModel model, string userName, string scope, Guid userPublicId)
        {
            string refreshToken = _jwtService.GenerateToken(REFRESH_TOKEN_LIFETIME, REFRESH_TOKEN_NAME, userName, model.ClientId, scope, userPublicId);
            _clientService.UpdateRefreshTokenForClient(refreshToken, model.ClientId, userName);

            return Json(new
            {
                access_token = _jwtService.GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, model.ClientId, scope, userPublicId),
                token_type = "bearer",
                expires_in = ACCESS_TOKEN_LIFETIME * 60, // en secondes
                refresh_token = refreshToken,
                scope = String.IsNullOrEmpty(scope) ? String.Empty : scope
            });
        }

        private bool CheckAuthorizationHeaderForRessourceServer(IRessourceServerService serv)
        {
            if (!Request.Headers.ContainsKey("Authorization") || Request.Headers["Authorization"].Count != 1)
                return false;

            string[] authsInfos = Request.Headers["Authorization"].First().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (authsInfos.Length != 2)
                return false;

            if (!authsInfos[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
                return false;

            return serv.AreRessourceServerCredentialsValid(authsInfos[1]);
        }

        private bool CheckAuthorizationHeader(out JsonResult result)
        {
            result = null;

            if (!Request.Headers.ContainsKey("Authorization") || Request.Headers["Authorization"].Count != 1)
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }

            string[] authsInfos = Request.Headers["Authorization"].First().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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

            if (!_clientService.AreClientCredentialsValid(authsInfos[1]))
            {
                result = GenerateErrorResponse(HttpStatusCode.Unauthorized, "unauthorized_client", "L'authentification du client a échoué");
                return false;
            }

            return true;
        }

        private static string GenerateRedirectErrorMessage(string redirectUri, string errorName, string errorDescription, string stateInfo)
        {
            if (String.IsNullOrEmpty(stateInfo))
                return GenerateRedirectErrorMessage(redirectUri, errorName, errorDescription);            

            Uri uri = new Uri(String.Format(CultureInfo.InvariantCulture, "{0}?error={1}&error_description={2}&state={3}", redirectUri, errorName, errorDescription, stateInfo));
            return uri.AbsoluteUri;
        }

        private static string GenerateRedirectErrorMessage(string redirectUri, string errorName, string errorDescription)
        {
            Uri uri = new Uri(String.Format(CultureInfo.InvariantCulture, "{0}?error={1}&error_description={2}", redirectUri, errorName, errorDescription));
            return uri.AbsoluteUri;
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

        private static bool IsUriCorrect(string uri)
        {
            return Uri.TryCreate(uri, UriKind.Absolute, out Uri u);
        }
    }

    #endregion
}
