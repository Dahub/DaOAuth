using DaOAuth.Dal.EF;
using DaOAuth.Service;
using System;
using System.Net;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    [HandleError]
    public class OAuthController : Controller
    {
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

            // si l'utilisateur n'est pas connecté, il faut l'inviter à le faire
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("/User/Login");

            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            if (!cs.IsClientValidForAuthorizationCodeGrant(client_id, redirect_uri))
                return Redirect(GenerateRedirectErrorMessage(redirect_uri, "unauthorized_client", "Le client ne possède pas les droits de demander une authorisation", state));


            // vérifier que l'utilisateur a bien autorisé le client, sinon, prompt d'autorisation

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        [Authorize]
        [Route("/token")]
        public JsonResult jsonResult()
        {
            return Json("ok");
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

        private bool IsUriCorrect(string uri)
        {
            Uri u = null;
            return Uri.TryCreate(uri, UriKind.Absolute, out u);
        }
    }
}