using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DaOAuth.Dal.EF
{
    internal class ClientRepository : IClientRepository
    {
        public IContext Context { get; set; }

        public void Add(Client toAdd)
        {
            ((DbContext)Context).Set<Client>().Add(toAdd);
        }

        public Client GetByPublicId(string publicId)
        {
            return ((DaOAuthContext)Context).Clients.
                Where(c => c.PublicId.Equals(publicId)).FirstOrDefault();
        }

        public void Update(Client toUpdate)
        {
            ((DbContext)Context).Set<Client>().Attach(toUpdate);
            ((DbContext)Context).Entry(toUpdate).State = EntityState.Modified;
        }

        public IEnumerable<Client> GetAllByUserName(string userName)
        {
            return ((DaOAuthContext)Context).UsersClients.
                Where(c => c.User.UserName.Equals(userName)).Select(c => c.Client).Include("Scopes");
        }
    }
}
