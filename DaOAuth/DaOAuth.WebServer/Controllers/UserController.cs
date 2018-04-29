using DaOAuth.Dal.EF;
using DaOAuth.Service;
using DaOAuth.WebServer.Models;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        public ActionResult Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var s = new UserService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            s.CreateUser(new UserDto()
            {
                BirthDate = model.BirthDate,
                FullName = model.FullName,
                UserName = model.UserName
            }, model.Password);

            return Redirect("/Home/Index");
        }
    }
}