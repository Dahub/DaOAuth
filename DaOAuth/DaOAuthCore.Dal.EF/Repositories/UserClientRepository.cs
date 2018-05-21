using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DaOAuthCore.Dal.EF
{
    public class UserClientRepository : IUserClientRepository
    {
        public IContext Context { get; set; }

        public void Add(UserClient userClient)
        {
            ((DbContext)Context).Set<UserClient>().Add(userClient);
        }

        public UserClient GetUserClientByUserNameAndClientPublicId(string clientPublicId, string userName)
        {
            return ((DaOAuthContext)Context).UsersClients.Where(uc => uc.Client.PublicId.Equals(clientPublicId) && uc.User.UserName.Equals(userName, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        public void Delete(UserClient userClient)
        {
            ((DbContext)Context).Set<UserClient>().Attach(userClient);
            ((DbContext)Context).Entry(userClient).State = EntityState.Deleted;
        }

        public void Update(UserClient userClient)
        {
            ((DbContext)Context).Set<UserClient>().Attach(userClient);
            ((DbContext)Context).Entry(userClient).State = EntityState.Modified;
        }

        public IEnumerable<UserClient> GetAllByUserName(string userName)
        {
            return ((DaOAuthContext)Context).UsersClients.
                Include(uc => uc.Client).
                Include(uc => uc.Client.ClientsScopes).
                ThenInclude(cs => cs.Scope).
                Where(uc => uc.User.UserName.Equals(userName, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
