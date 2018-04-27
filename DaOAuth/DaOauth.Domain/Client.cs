using System;
using System.Collections.Generic;

namespace DaOAuth.Domain
{
    public class Client
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public byte[] ClientSecret { get; set; }
        public string Name { get; set; }
        public string DefautRedirectUri { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsValid { get; set; }
        public string RefreshToken { get; set; }

        public ICollection<Code> Codes { get; set; }

        public int ClientTypeId { get; set; }
        public ClientType ClientType { get; set; }
    }
}
