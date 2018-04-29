using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaOAuth.Dal.Fake
{
    internal class UserRepository : IUserRepository
    {
        public IContext Context { get; set; }

        public void Add(User toAdd)
        {

        }

        public User GetByUserName(string userName)
        {
            return new User()
            {
                BirthDate = new DateTime(1978, 9, 16),
                CreationDate = DateTime.Now,
                FullName = "Samy Le Crabe",
                Id = 1,
                IsValid = true,
                Password = new byte[] { 0 },
                UserName = "Sam"
            };
        }

        public void Update(User toUpdate)
        {

        }
    }
}
