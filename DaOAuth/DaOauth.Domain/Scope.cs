﻿namespace DaOAuth.Domain
{
    public class Scope
    {
        public int Id { get; set; }
        public string Wording { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
    }
}