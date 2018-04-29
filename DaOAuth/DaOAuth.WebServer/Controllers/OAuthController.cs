using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    public class OAuthController : Controller
    {
        // GET: OAuth
        public ActionResult Authorize(string response_type, string client_id, string state, string redirect_uri)
        {
            return View();
        }
    }
}