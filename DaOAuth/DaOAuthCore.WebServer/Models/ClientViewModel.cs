using System.Collections.Generic;

namespace DaOAuthCore.WebServer.Models
{
    public class ClientViewModel
    {
        public int? ClientId { get; set; }
        public string PublicId { get; set; }
        public string Secret { get; set; }
        public int ClientTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefautRedirectUri { get; set; }
        public IEnumerable<string> Scopes { get; set; }
    }
}
