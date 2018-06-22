using Microsoft.AspNetCore.Mvc;

namespace DaOAuthCore.WebServer.Models
{
    [ModelBinder(BinderType = typeof(Models.Binders.ChangeAuthorizationClientModelBinder), Name = "ChangeAuthorizationClientModel")]
    public class ChangeAuthorizationClientModel
    {
        public string ClientId { get; set; }
        public bool Authorize { get; set; }
    }
}
