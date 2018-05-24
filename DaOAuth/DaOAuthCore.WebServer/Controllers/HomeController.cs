using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DaOAuthCore.WebServer.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
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
