using System.Collections.Generic;

namespace DaOAuthCore.Domain
{
    public class ClientType
    {
        public int Id { get; set; }
        public string Wording { get; set; }

        public ICollection<Client> Clients { get; set; }
    }
}
