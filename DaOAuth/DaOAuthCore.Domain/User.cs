using System;
using System.Collections.Generic;

namespace DaOAuthCore.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public byte[] Password { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool IsValid { get; set; }
        public ICollection<UserClient> UsersClients { get; set; }
    }
}
