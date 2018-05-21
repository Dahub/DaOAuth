using System.Collections.Generic;

namespace DaOAuthCore.Domain
{
    public class Scope
    {
        public int Id { get; set; }
        public string Wording { get; set; }
        public string NiceWording { get; set; }
        public ICollection<ClientScope> ClientsScopes { get; set; }
    }
}
