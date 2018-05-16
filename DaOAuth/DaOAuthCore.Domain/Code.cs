using System;

namespace DaOAuthCore.Domain
{
    public class Code
    {
        public int Id { get; set; }
        public string CodeValue { get; set; }
        public long ExpirationTimeStamp { get; set; } // new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds()
        public bool IsValid { get; set; }
        public string Scope { get; set; }
        public string UserName { get; set; }
        public Guid UserPublicId { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}

