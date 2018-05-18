using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace DaOAuthCore.WebServer.Controllers
{
    public class OAuthController : Controller
    {
        private IClientService _clientService;
        private IUserService _userService;
        private AppConfiguration _configuration;

        private const int ACCESS_TOKEN_LIFETIME = 10; // temps en minutes
        private const int REFRESH_TOKEN_LIFETIME = 10 * 365 * 24 * 60; // 5256000 => 10 ans exprimés en minutes
        private const string ACCESS_TOKEN_NAME = "access_token";
        private const string REFRESH_TOKEN_NAME = "refresh_token";

        public OAuthController([FromServices] IClientService cs, [FromServices] IUserService us, IOptions<AppConfiguration> conf)
        {
            _clientService = cs;
            _userService = us;
            _configuration = conf.Value;
        }

        [AllowAnonymous]
        [Route("/authorize")]
        public ActionResult Authorize(string response_type, string client_id, string state, string redirect_uri, string scope)
        {
            if (String.IsNullOrEmpty(redirect_uri))
                return StatusCode((int)HttpStatusCode.BadRequest, "le paramètre redirect_uri doit être renseigné");

            if (!IsUriCorrect(redirect_uri))
                return StatusCode((int)HttpStatusCode.BadRequest, "l'url de redirection est incorrecte");

            if (String.IsNullOrEmpty(response_type))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_request", "Le paramètre response_type est requis", state));

            if (String.IsNullOrEmpty(client_id))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_request", "Le paramètre client_id est requis", state));

            if (!_clientService.IsClientValidForAuthorization(client_id, redirect_uri, response_type))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", state));

            // vérification des scopes proposés
            if (!_clientService.AreScopesAuthorizedForClient(client_id, scope))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "invalid_scope", "Scopes demandés invalides"));

            // si l'utilisateur n'est pas connecté, il faut l'inviter à le faire
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("LoginAuthorize", "User", new
                {
                    response_type = response_type,
                    client_id = client_id,
                    state = state,
                    redirect_uri = redirect_uri,
                    scope = scope
                });

            // vérifier que l'utilisateur a bien connaissance du client, sinon, prompt d'autorisation
            if (!_clientService.HasUserAuthorizeOrDeniedClientAccess(client_id, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value))
                return RedirectToAction("AuthorizeClient", "User", new
                {
                    response_type = response_type,
                    client_id = client_id,
                    state = state,
                    redirect_uri = redirect_uri,
                    scope = scope
                });

            // l'utilisateur a t'il authorisé le client ?
            string userName = ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;

            Guid userPublicId;

            if (!_clientService.IsClientAuthorizeByUser(client_id, userName, out userPublicId))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "access_denied", "L'utilisateur a refusé l'accès au client", state));

            switch (response_type)
            {
                case "code":
                    return RedirectForResponseTypeCode(client_id, state, redirect_uri, userName, scope, userPublicId);
                case "token": // implicit grant pour spa applications
                    return RedirectForResponseTypeToken(client_id, state, redirect_uri, userName, scope, userPublicId);
                default:
                    return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unsupported_response_type", "La valeur du paramètre response_type doit être code", state));
            }
        }

        [HttpPost]
        [Route("/token")]
        public JsonResult Token([FromBody] TokenModel model)
        {
            try
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
        public JsonResult Introspect([FromBody] IntrospectTokenModel model)
        {
            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.token))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "le token est obligatoire");

            ClaimsPrincipal user;
            long expire = 0;
            if (!CheckIfTokenIsValid(model.token, ACCESS_TOKEN_NAME, out expire, out user))
            {
                return Json(new
                {
                    active = false
                });
            }

            // on récupère les infos
            string scope = GetValueFromClaim(user.Claims, "scope");
            var clientId = GetValueFromClaim(user.Claims, "client_id");
            var userName = GetValueFromClaim(user.Claims, ClaimTypes.NameIdentifier);
            var userPublicId = GetValueFromClaim(user.Claims, "user_public_id");

            return Json(new
            {
                active = true,
                client_id = clientId,
                username = userName,
                scope = scope,
                exp = expire,
                user_public_id = userPublicId
            });
        }

        #region private

        private bool CheckIfTokenIsValid(string token, string token_name, out long expire, out ClaimsPrincipal user)
        {
            expire = 0;
            user = null;

            if (String.IsNullOrEmpty(token))
                return false;

            var handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = "DaOAuth",
                ValidAudience = "DaOAuth",                
                IssuerSigningKeys = new List<SecurityKey>() { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.SecurityKey)) }
            };

            SecurityToken validatedToken;
            try
            {
                user = handler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch
            {
                return false;
            }     

            long.TryParse(GetValueFromClaim(user.Claims, "exp"), out expire);

            if (expire < DateTimeOffset.Now.ToUnixTimeSeconds() || GetValueFromClaim(user.Claims, "token_name") != token_name)
                return false;

            return true;
        }

        private ActionResult RedirectForResponseTypeToken(string clientId, string state, string redirectUri, string userName, string scope, Guid userPublicId)
        {
            string location = String.Empty;
            var myToken = GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, clientId, scope, userPublicId);
            location = String.Concat(redirectUri, "?token=", myToken, "?token_type=bearer?expires_in", ACCESS_TOKEN_LIFETIME * 60);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(location);
        }

        private ActionResult RedirectForResponseTypeCode(string clientId, string state, string redirectUri, string userName, string scope, Guid userPublicId)
        {
            string location = String.Empty;
            var myCode = _clientService.GenerateAndAddCodeToClient(clientId, userName, scope, userPublicId);
            location = String.Concat(redirectUri, "?code=", myCode);
            if (!String.IsNullOrEmpty(state))
                location = String.Concat(location, "&state=", state);
            return Redirect(location);
        }

        private JsonResult GenerateTokenForClientCredentailsGrant(TokenModel model)
        {
            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError))
                return toReturnIfError;

            var clientId = _clientService.GetClientIdFromAuthorizationHeaderValue(Request.Headers["Authorization"]);

            if (!_clientService.AreScopesAuthorizedForClient(model.client_id, model.scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            return Json(new
            {
                access_token = GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, clientId, model.scope, null),
                token_type = "bearer",
                expires_in = ACCESS_TOKEN_LIFETIME * 60
            });
        }

        private JsonResult GenerateTokenForRefreshToken(TokenModel model)
        {
            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.refresh_token))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "refresh_token", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");

            // analyse du refresh token
            ClaimsPrincipal user;
            long expire = 0;
            if (!CheckIfTokenIsValid(model.refresh_token, REFRESH_TOKEN_NAME, out expire, out user))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");         

            // on récupère les infos
            string scope = GetValueFromClaim(user.Claims, "scope");
            var clientId = GetValueFromClaim(user.Claims, "client_id");
            var userName = GetValueFromClaim(user.Claims, ClaimTypes.NameIdentifier);
            Guid userPublicId = Guid.Parse(GetValueFromClaim(user.Claims, "user_public_id"));

            // vérifier que le token n'ai pas été révoqué
            if (!_clientService.IsRefreshTokenValid(userName, clientId, model.refresh_token))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "Le refresh token est invalide");

            // générer un nouveau token (et refresh token à mettre à jour)
            model.client_id = clientId;

            if (!_clientService.AreScopesAuthorizedForClient(model.client_id, scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            return GenerateAccesTokenAndUpdateRefreshToken(model, userName, scope, userPublicId);
        }

        private JsonResult GenerateTokenForAuthorizationCodeGrant(TokenModel model)
        {
            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.code))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre code doit être présent une et une seule fois et avoir une valeur");

            if (String.IsNullOrEmpty(model.redirect_uri))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre redirect_uri doit être présent une et une seule fois et avoir une valeur");

            if (String.IsNullOrEmpty(model.client_id))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur");

            if (!_clientService.IsClientValidForAuthorization(model.client_id, model.redirect_uri, "code"))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_client", "client non valide");

            CodeDto codeInfos = _clientService.GetCodeInfos(model.client_id, model.code);

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

            JsonResult toReturnIfError;
            if (!CheckAuthorizationHeader(out toReturnIfError))
                return toReturnIfError;

            if (String.IsNullOrEmpty(model.username))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre username doit être présent une et une seule fois et avoir une valeur");

            if (String.IsNullOrEmpty(model.password))
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_request", "Le paramètre password doit être présent une et une seule fois et avoir une valeur");

            // vérification de la validé des identifiants          
            if (_userService.Find(model.username, model.password) == null)
                return GenerateErrorResponse(HttpStatusCode.BadRequest, "invalid_grant", "username ou password incorrect");

            model.client_id = _clientService.GetClientIdFromAuthorizationHeaderValue(Request.Headers["Authorization"]);

            if (!_clientService.AreScopesAuthorizedForClient(model.client_id, model.scope))
                return GenerateErrorResponse(HttpStatusCode.Unauthorized, "invalid_scope", "Scopes demandés invalides");

            Guid userPublicId = _clientService.GetUserPublicId(model.client_id, model.username);

            return GenerateAccesTokenAndUpdateRefreshToken(model, model.username, model.scope, userPublicId);
        }

        private JsonResult GenerateAccesTokenAndUpdateRefreshToken(TokenModel model, string userName, string scope, Guid userPublicId)
        {
            string refreshToken = GenerateToken(REFRESH_TOKEN_LIFETIME, REFRESH_TOKEN_NAME, userName, model.client_id, scope, userPublicId);
            _clientService.UpdateRefreshTokenForClient(refreshToken, model.client_id, userName);

            return Json(new
            {
                access_token = GenerateToken(ACCESS_TOKEN_LIFETIME, ACCESS_TOKEN_NAME, userName, model.client_id, scope, userPublicId),
                token_type = "bearer",
                expires_in = ACCESS_TOKEN_LIFETIME * 60, // en secondes
                refresh_token = refreshToken,
                scope = String.IsNullOrEmpty(scope) ? String.Empty : scope
            });
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

        private string GenerateToken(int minutesLifeTime, string tokenName, string clientId, string scope, Guid? userPublicId)
        {
            return GenerateToken(minutesLifeTime, tokenName, String.Empty, clientId, scope, userPublicId);
        }

        private string GenerateToken(int minutesLifeTime, string tokenName, string userName, string clientId, string scope, Guid? userPublicId)
        {
            IList<Claim> claims = new List<Claim>();
            claims.Add(new Claim("client_id", clientId));
            claims.Add(new Claim("token_name", tokenName));
            claims.Add(new Claim("issued", DateTimeOffset.Now.ToUnixTimeSeconds().ToString()));
            claims.Add(new Claim("user_public_id", userPublicId.HasValue ? userPublicId.Value.ToString() : String.Empty));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, !String.IsNullOrEmpty(userName) ? userName : String.Empty));
            claims.Add(new Claim("scope", !String.IsNullOrEmpty(scope) ? scope : String.Empty));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "DaOAuth",
                audience: "DaOAuth",
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.Now.AddMinutes(minutesLifeTime));

            return new JwtSecurityTokenHandler().WriteToken(token);
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

        private string GetValueFromClaim(IEnumerable<Claim> claims, string claimType)
        {
            var claim = claims.Where(c => c.Type.Equals(claimType)).FirstOrDefault();

            if (claim == null)
                return String.Empty;

            return claim.Value;
        }
    }

    #endregion
}
