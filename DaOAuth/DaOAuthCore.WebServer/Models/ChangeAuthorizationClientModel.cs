namespace DaOAuthCore.WebServer.Models
{
    public class ChangeAuthorizationClientModel
    {
        public string client_id { get; set; }
        public bool authorize { get; set; }
    }
}
