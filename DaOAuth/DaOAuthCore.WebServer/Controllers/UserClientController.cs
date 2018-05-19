using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace DaOAuthCore.WebServer.Controllers
{
    public class UserClientController : Controller
    {
        private IUserClientService _clientService;

        public UserClientController([FromServices] IUserClientService cs)
        {
            _clientService = cs;
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetClients()
        {
            return Json(_clientService.GetUserClientsByUserName(((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value));
        }

        [Authorize]
        [HttpPost]
        public void RevokeClient(RevokeClientModel model)
        {
            _clientService.RevokeClient(model.client_id, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [Authorize]
        [HttpPost]
        public void ChangeAuthorizationClient(ChangeAuthorizationClientModel model)
        {
            _clientService.ChangeAuthorizationClient(model.client_id, model.authorize, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}