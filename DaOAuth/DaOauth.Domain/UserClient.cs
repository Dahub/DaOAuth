using System;

namespace DaOAuth.Domain
{
    public class UserClient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public DateTime CreationDate { get; set; }
        public int UserPublicId { get; set; }
    }
}



