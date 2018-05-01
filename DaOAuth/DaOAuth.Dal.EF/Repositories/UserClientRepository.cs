using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System.Linq;
using System.Data.Entity;

namespace DaOAuth.Dal.EF
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
    }
}
