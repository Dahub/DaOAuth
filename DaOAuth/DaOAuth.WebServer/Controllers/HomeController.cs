using DaOAuth.Dal.EF;
using DaOAuth.Service;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetClients()
        {
            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            return Json(cs.GetClientsByUserName(((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.NameIdentifier)), JsonRequestBehavior.AllowGet);
        }
    }
}