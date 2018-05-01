using DaOAuth.Dal.EF;
using DaOAuth.Service;
using DaOAuth.WebServer.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace DaOAuth.WebServer.Controllers
{
    [HandleError]
    public class UserController : Controller
    {
        [Authorize]
        [HttpGet]
        public ActionResult AuthorizeClient(string response_type, string client_id, string state, string redirect_uri)
        {
            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            return View(new AuthorizeClientViewModel()
            {
                ClientId = client_id,
                RedirectUrl = redirect_uri,
                ResponseType = response_type,
                State = state,
                ClientName = cs.GetClientByPublicId(client_id).Name
            });
        }

        [Authorize]
        [HttpPost]
        public ActionResult AuthorizeClient(AuthorizeClientViewModel model)
        {
            var cs = new ClientService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            cs.AuthorizeClientForUser(model.ClientId, ((ClaimsIdentity)User.Identity).FindFirstValue(ClaimTypes.NameIdentifier));

            return RedirectToAction("authorize", "OAuth", new
            {
                response_type = model.ResponseType,
                client_id = model.ClientId,
                state = model.State,
                redirect_uri = model.RedirectUrl
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LoginAuthorize(string response_type, string client_id, string state, string redirect_uri)
        {
            return View(new LoginAuthorizeViewModel()
            {
                ClientId = client_id,
                RedirectUrl = redirect_uri,
                ResponseType = response_type,
                State = state
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string userName, string password)
        {
            var s = new UserService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            UserDto user = s.Find(userName, password);

            if (user != null)
                LogUser(user);

            return Json(new
            {
                logged = user != null ? "true" : "false"
            });
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("/Home/Index");

            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(UserCreateViewModel model)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("/Home/Index");

            if (!ModelState.IsValid)
                return View(model);

            var s = new UserService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            var created = s.CreateUser(new UserDto()
            {
                BirthDate = model.BirthDate,
                FullName = model.FullName,
                UserName = model.UserName
            }, model.Password);

            LogUser(created);

            return Redirect("/Home/Index");
        }

        [Authorize]
        public ActionResult Logout()
        {
            Request.GetOwinContext().Authentication.SignOut();

            return Redirect("/Home/Index");
        }

        private void LogUser(UserDto u)
        {
            var loginClaim = new Claim(ClaimTypes.NameIdentifier, u.UserName);
            var fullNameClaim = new Claim(ClaimTypes.Name, String.IsNullOrEmpty(u.FullName) ? "Non précisé":u.FullName);
            var birthDayClaim = new Claim(ClaimTypes.DateOfBirth, u.BirthDate.HasValue?u.BirthDate.Value.ToShortDateString():"Non précisée");
            var claimsIdentity = new ClaimsIdentity(new[] { loginClaim, fullNameClaim, birthDayClaim }, DefaultAuthenticationTypes.ApplicationCookie);
            var ctx = Request.GetOwinContext();
            var authenticationManager = ctx.Authentication;
            authenticationManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties()
            {
                ExpiresUtc = DateTime.Now.AddYears(100),
                IsPersistent = true,
                IssuedUtc = DateTime.Now
            }, claimsIdentity);
        }
    }
}