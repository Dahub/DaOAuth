using System.Collections.Generic;

namespace DaOAuth.Domain
{
    public class ClientType
    {
        public int Id { get; set; }
        public string Wording { get; set; }

        public ICollection<Client> Clients { get; set; }
    }
}
