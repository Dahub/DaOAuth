﻿namespace DaOAuth.WebServer.Models
{
    public class TokenModel
    {
        public string grant_type { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public string client_id { get; set; }
        public string refresh_token { get; set; }
    }
}