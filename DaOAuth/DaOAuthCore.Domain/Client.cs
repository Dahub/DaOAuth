using System;
using System.Collections.Generic;

namespace DaOAuthCore.Domain
{
    public class Client
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public byte[] ClientSecret { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DefautRedirectUri { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsValid { get; set; }
        public ICollection<Code> Codes { get; set; }
        public int ClientTypeId { get; set; }
        public ClientType ClientType { get; set; }
        public ICollection<UserClient> UsersClients { get; set; }
        public ICollection<ClientScope> ClientsScopes { get; set; }
    }
}
