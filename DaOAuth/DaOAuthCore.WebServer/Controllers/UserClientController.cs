using DaOAuthCore.Service;
using DaOAuthCore.WebServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult RevokeClient(RevokeClientModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

             _clientService.RevokeClient(model.ClientId, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value);
            
            return Ok();
        }

        [Authorize]
        [HttpPost]
        public void ChangeAuthorizationClient(ChangeAuthorizationClientModel model)
        {
            _clientService.ChangeAuthorizationClient(model.ClientId, model.Authorize, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value);
        }
    }
}