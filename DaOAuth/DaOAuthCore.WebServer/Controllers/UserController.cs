using DaOAuthCore.Dal.EF;
using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace DaOAuthCore.WebServer.Controllers
{
    public class UserController : Controller
    {
        private IClientService _clientService;
        private IUserService _userService;

        public UserController([FromServices] IClientService cs, [FromServices] IUserService us)
        {
            _clientService = cs;
            _userService = us;
        }

        [Authorize]
        [HttpGet]
        public ActionResult AuthorizeClient(string response_type, string client_id, string state, string redirect_uri, string scope)
        {        
            return View(new AuthorizeClientViewModel()
            {
                ClientId = client_id,
                RedirectUrl = redirect_uri,
                ResponseType = response_type,
                State = state,
                ClientName = _clientService.GetClientByPublicId(client_id).Name,
                IsValid = true,
                Scope = scope
            });
        }

        [Authorize]
        [HttpPost]
        public ActionResult AuthorizeClient(AuthorizeClientViewModel model)
        {
            _clientService.AuthorizeOrDeniedClientForUser(model.ClientId, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value, model.IsValid);

            return RedirectToAction("authorize", "OAuth", new
            {
                response_type = model.ResponseType,
                client_id = model.ClientId,
                state = model.State,
                redirect_uri = model.RedirectUrl,
                scope = model.Scope
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LoginAuthorize(string response_type, string client_id, string state, string redirect_uri, string scope)
        {
            return View(new LoginAuthorizeViewModel()
            {
                ClientId = client_id,
                RedirectUrl = redirect_uri,
                ResponseType = response_type,
                State = state,
                Scope = scope
            });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string userName, string password)
        {
            UserDto user = _userService.Find(userName, password);

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
            
            var created = _userService.CreateUser(new UserDto()
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
            HttpContext.SignOutAsync("DaOAuth");
            return Redirect("/Home/Index");
        }

        private void LogUser(UserDto u)
        {
            var loginClaim = new Claim(ClaimTypes.NameIdentifier, u.UserName);
            var fullNameClaim = new Claim(ClaimTypes.Name, String.IsNullOrEmpty(u.FullName) ? "Non précisé" : u.FullName);
            var birthDayClaim = new Claim(ClaimTypes.DateOfBirth, u.BirthDate.HasValue ? u.BirthDate.Value.ToShortDateString() : "Non précisée");
            var claimsIdentity = new ClaimsIdentity(new[] { loginClaim, fullNameClaim, birthDayClaim }, "cookie");
            ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);
            HttpContext.SignInAsync("DaOAuth", principal, new AuthenticationProperties()
            {                
                ExpiresUtc = DateTime.Now.AddYears(100),
                IsPersistent = true,
                IssuedUtc = DateTime.Now
            });
        }
    }
}