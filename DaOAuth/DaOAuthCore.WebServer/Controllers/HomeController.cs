using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
