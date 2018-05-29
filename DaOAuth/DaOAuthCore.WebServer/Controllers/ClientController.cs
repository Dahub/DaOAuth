using DaOAuthCore.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DaOAuthCore.WebServer.Controllers
{
    public class ClientController : Controller
    {
        private IClientService _clientService;

        public ClientController([FromServices] IClientService cs)
        {
            _clientService = cs;
        }
    }
}