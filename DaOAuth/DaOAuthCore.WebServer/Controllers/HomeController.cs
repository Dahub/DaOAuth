using DaOAuthCore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DaOAuthCore.WebServer.Controllers
{
    public class HomeController : Controller
    {
        private IUserClientService _clientService;

        public HomeController([FromServices] IUserClientService cs)
        {
            _clientService = cs;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetClients()
        {
            return Json(_clientService.GetUserClientsByUserName(((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value));
        }
    }
}
