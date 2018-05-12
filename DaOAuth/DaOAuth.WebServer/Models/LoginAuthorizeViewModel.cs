namespace DaOAuth.WebServer.Models
{
    public class LoginAuthorizeViewModel
    {
        public string ResponseType { get; set; }
        public string ClientId { get; set; }
        public string State { get; set; }
        public string RedirectUrl { get; set; }
        public string Scope { get; set; }
    }
}