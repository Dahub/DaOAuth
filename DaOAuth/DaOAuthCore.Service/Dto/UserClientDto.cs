﻿namespace DaOAuthCore.Service
{
    public class UserClientDto
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientDescription { get; set; }
        public bool IsAuthorize { get; set; }
        public string[] ScopesNiceWordings { get; set; }
    }
}
