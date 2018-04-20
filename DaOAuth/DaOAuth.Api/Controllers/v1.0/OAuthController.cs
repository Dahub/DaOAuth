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
                var states = Request.GetQueryNameValuePairs().Where(k => k.Key.Equals("state")).Select(v => v.Value).ToList();
                if (states.Count > 1)
                    return GenerateErrorResponse("invalid_request", "Le paramètre state doit être présent une et une seule fois");
                string myState = String.Empty;
                if (states.Count == 1)
                    myState = states.First();

                // response type
                var response_types = Request.GetQueryNameValuePairs().Where(k => k.Key.Equals("response_type")).Select(v => v.Value).ToList();
                if (response_types == null || response_types.Count != 1)
                    return GenerateErrorResponse("invalid_request", "Le paramètre response_type doit être présent une et une seule fois et avoir une valeur", myState);
                string response_type = response_types.First();
                if (response_type != "code")
                    return GenerateErrorResponse("unsupported_response_type", "La valeur du paramètre response_type doit être code", myState);

                // client_id
                var client_ids = Request.GetQueryNameValuePairs().Where(k => k.Key.Equals("client_id")).Select(v => v.Value).ToList();
                if (client_ids == null || client_ids.Count != 1)
                    return GenerateErrorResponse("invalid_request", "Le paramètre client_id doit être présent une et une seule fois et avoir une valeur", myState);
                string client_id = client_ids.First();                   

                ClientService cs = new ClientService()
                {
                    ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                    Factory = new EfRepositoriesFactory()
                };
                if (!cs.CheckIfClientIsValid(client_id))
                    return GenerateErrorResponse("unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", myState);

                // tout est ok, on peut générer un nouveau code pour cette demande
                var myCode = cs.AddCodeToClient(client_id);

                // var response = Request.CreateResponse(HttpStatusCode.Moved);
                //response.Headers.Location = new Uri("http://www.abcmvc.com");
                //return response;
                //if (String.IsNullOrEmpty(red))
                if (String.IsNullOrEmpty(myState))
                    return Ok(new { code = myCode.CodeValue });
                
                else
                    return Ok(new { code = myCode.CodeValue , state = myState });
            }
            catch(Exception ex)
            {
                /* The authorization server encountered an unexpected
                condition that prevented it from fulfilling the request.
                (This error code is needed because a 500 Internal Server
                Error HTTP status code cannot be returned to the client
                via an HTTP redirect.) */
                return GenerateErrorResponse("server_error", String.Format("Une erreur s'est produite : {0}", ex.Message));
            }
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
