using Microsoft.AspNetCore.Mvc;
using System;

namespace DaOAuthCore.WebServer.Models
{
    [ModelBinder(BinderType = typeof(Models.Binders.TokenModelBinder), Name = "TokenModel")]
    public class TokenModel
    {       
        public string GrantType { get; set; }
        public string Code { get; set; }
        public Uri RedirectUrl { get; set; }
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Scope { get; set; }
    }
}