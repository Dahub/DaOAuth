using Microsoft.AspNetCore.Mvc;

namespace DaOAuthCore.WebServer.Models
{
    [ModelBinder(BinderType = typeof(Models.Binders.RevokeClientModelBinder), Name = "RevokeClientModel")]
    public class RevokeClientModel
    {
        public string ClientId { get; set; }
    }
}
