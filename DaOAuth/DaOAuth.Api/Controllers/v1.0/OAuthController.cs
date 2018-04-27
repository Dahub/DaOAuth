using DaOAuth.Dal.EF;
using DaOAuth.Service;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataHandler.Serializer;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace DaOAuth.Api.Controllers.V1_0
{
    /// <summary>
    /// Controller implémentant les endpoints d'un serveur d'authorisation OAuth2 
    /// RFC 6749
    /// </summary>
    [ApiVersion("1.0")]
    [RoutePrefix("auth/v{version:ApiVersion}")]
    public class OAuthController : ApiController
    {
        private const int ACCESS_TOKEN_LIFETIME = 10;
        private const int REFRESH_TOKEN_LIFETIME = 525600; // un an

        [HttpGet]
        [Route("authorize")]
        public IHttpActionResult Authorize(string response_type)
        {
            try
            {
                /* vérification des paramètres d'entrée
                sont attendus : "response_type" , "client_id"
                en optionel : "redirect_uri", "scope"
                en recommandé : "state" */
                bool isError = false;
                string errorMsg = String.Empty;

                // state
                string myState = String.Empty;
                if (!TryExtractUniqueRequestParamValue("state", false, out myState))
                {
                    errorMsg = GenerateErrorMessage("invalid_request", "Le paramètre state doit être présent une et une seule fois");
                    isError = true;
                }

                // response type
                response_type = String.Empty;
                if (!isError && !TryExtractUniqueRequestParamValue("response_type", true, out response_type))
                {
                    errorMsg = GenerateErrorMessage("invalid_request", "Le paramètre response_type doit être présent une et une seule fois et avoir une valeur", myState);
                    isError = true;
                }
                if (!isError && response_type != "code")
                {
                    errorMsg = GenerateErrorMessage("unsupported_response_type", "La valeur du paramètre response_type doit être code", myState);
                    isError = true;
                }

                // client_id
                string client_id = String.Empty;
                if (!TryExtractUniqueRequestParamValue("client_id", true, out client_id))
                {
                    errorMsg = GenerateErrorMessage("invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur", myState);
                    isError = true;
                }

                // redirect url
                string redirectUri = String.Empty;
                if (!TryExtractUniqueRequestParamValue("redirect_uri", true, out redirectUri))
                {
                    errorMsg = GenerateErrorMessage("invalid_request", "Le paramètre redirect_uri doit être présent une et une seule fois et avoir une valeur", myState);
                    isError = true;
                }

                ClientService cs = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };

                var clientInfos = cs.IsClientValidForAuthorizationCodeGrant(client_id, redirectUri);
                if (!clientInfos)
                {
                    errorMsg = GenerateErrorMessage("unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", myState);
                    isError = true;
                }

                string location = String.Empty;

                // tout est ok, on peut générer un nouveau code pour cette demande
                if (!isError)
                {
                    var myCode = cs.AddCodeToClient(client_id);
                    location = String.Concat(redirectUri, "?code=", myCode.CodeValue);
                    if (!String.IsNullOrEmpty(myState))
                        location = String.Concat(location, "&state=", myState);
                }
                else
                {
                    location = String.Concat(redirectUri, "?", errorMsg);
                }

                // Création de la response de redirection
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                Uri myUri;
                if (!Uri.TryCreate(location, UriKind.Absolute, out myUri))
                    return BadRequest("l'adresse de redirection est invalide"); // seul cas ou on redirige pas : aucune adresse de redirection
                response.Headers.Location = myUri;

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("token")]
        public IHttpActionResult Token(TokenModel model)
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
            return Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
        }

        private IHttpActionResult GenerateTokenForAuthorizationCodeGrant(TokenModel model)
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
                if (Request.Headers.Authorization == null ||
                    !Request.Headers.Authorization.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
                    !s.AreClientCredentialsValid(Request.Headers.Authorization.Parameter))
                    return GenerateErrorResponse("unauthorized_client", "L'authentification du client a échoué");

                //   Request.Headers.Authorization.

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

                return Ok(new
                {
                    access_token = GenerateToken(ACCESS_TOKEN_LIFETIME),
                    token_type = "bearer",
                    expires_in = ACCESS_TOKEN_LIFETIME * 60,
                    refresh_token = refreshToken
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private bool TryExtractUniqueRequestParamValue(string paramName, bool mustExist, out string value)
        {
            value = String.Empty;

            var values = Request.GetQueryNameValuePairs().Where(k => k.Key.Equals(paramName)).Select(v => v.Value).ToList();

            if (mustExist & values.Count == 0)
                return false;

            if (values.Count > 1)
                return false;

            if (values.Count == 1)
                value = values.First();

            return true;
        }

        private string GenerateErrorMessage(string errorName, string errorDescription, string stateInfo)
        {
            if (String.IsNullOrEmpty(stateInfo))
                return GenerateErrorMessage(errorName, errorDescription);

            return String.Format("error={0}&error_description={1}&state={2}", errorName, errorDescription, stateInfo);
        }

        private string GenerateErrorMessage(string errorName, string errorDescription)
        {
            return String.Format("error={0}&error_description={1}", errorName, errorDescription);
        }

        private IHttpActionResult GenerateErrorResponse(string errorName, string errorDescription, string stateInfo)
        {
            if (String.IsNullOrEmpty(stateInfo))
                return GenerateErrorResponse(errorName, errorDescription);

            var myError = new
            {
                error = errorName,
                error_description = errorDescription,
                state = stateInfo
            };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, myError));
        }

        private IHttpActionResult GenerateErrorResponse(string errorName, string errorDescription)
        {
            var myError = new
            {
                error = errorName,
                error_description = errorDescription
            };

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, myError));
        }
    }
}
