using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaOAuth.Dal.Fake
{
    public class UserClientRepository : IUserClientRepository
    {
        public IContext Context { get; set; }

        public void Add(UserClient userClient)
        {
            
        }

        public UserClient GetUserClientByUserNameAndClientPublicId(string clientPublicId, string userName)
        {
            if (clientPublicId == "abc")
                return null;

            return new UserClient()
            {
                Id = 1 
            };
        }

        public void Delete(UserClient userClient)
        {
            
        }
    }
}
