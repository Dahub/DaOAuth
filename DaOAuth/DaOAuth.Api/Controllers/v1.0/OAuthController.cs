using DaOAuth.Dal.EF;
using DaOAuth.Service;
using Microsoft.Web.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        [HttpGet]
        [Route("authorize")]
        public IHttpActionResult Authorize()
        {
            try
            {
                /* vérification des paramètres d'entrée
                sont attendus : "response_type" , "client_id"
                en optionel : "redirect_uri", "scope"
                en recommandé : "state" */

                // state
                string myState = String.Empty;
                if (!TryExtractUniqueRequestParamValue("state", false, out myState))
                    return GenerateErrorResponse("invalid_request", "Le paramètre state doit être présent une et une seule fois");

                // response type
                string response_type = String.Empty;
                if (!TryExtractUniqueRequestParamValue("response_type", true, out response_type))
                    return GenerateErrorResponse("invalid_request", "Le paramètre response_type doit être présent une et une seule fois et avoir une valeur", myState);
                if (response_type != "code")
                    return GenerateErrorResponse("unsupported_response_type", "La valeur du paramètre response_type doit être code", myState);

                // client_id
                string client_id = String.Empty;
                if (!TryExtractUniqueRequestParamValue("client_id", true, out client_id))
                    return GenerateErrorResponse("invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur", myState);

                // redirect url
                string redirectUri = String.Empty;
                if (!TryExtractUniqueRequestParamValue("redirect_uri", false, out redirectUri))
                    return GenerateErrorResponse("invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur", myState);
                
                ClientService cs = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };
                var clientInfos = cs.GetClientInfoForAuthorizationCodeGrant(client_id, redirectUri);

                if (!clientInfos.IsValid)
                    return GenerateErrorResponse("unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", myState);

                // tout est ok, on peut générer un nouveau code pour cette demande
                var myCode = cs.AddCodeToClient(client_id);

                string location = String.Concat(clientInfos.RedirectUri, "?code=", myCode.CodeValue);
                if (!String.IsNullOrEmpty(myState))
                    location = String.Concat(location, "&state=", myState);

                // Création de la response de redirection
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                Uri myUri;
                if (!Uri.TryCreate(location, UriKind.Absolute, out myUri))
                    return GenerateErrorResponse("invalid_request", "l'adresse de redirection est invalide", myState);
                response.Headers.Location = myUri;

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                /* The authorization server encountered an unexpected
                condition that prevented it from fulfilling the request.
                (This error code is needed because a 500 Internal Server
                Error HTTP status code cannot be returned to the client
                via an HTTP redirect.) */
                return GenerateErrorResponse("server_error", String.Format("Une erreur s'est produite : {0}", ex.Message));
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
