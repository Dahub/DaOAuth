using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaOAuth.Dal.EF
{
    internal class ScopeRepository : IScopeRepository
    {
        public IContext Context { get; set; }

        public void Add(Scope toAdd)
        {
            ((DbContext)Context).Set<Scope>().Add(toAdd);
        }

        public void Delete(Scope toDelete)
        {
            ((DbContext)Context).Set<Scope>().Attach(toDelete);
            ((DbContext)Context).Entry(toDelete).State = EntityState.Deleted;
        }

        public IEnumerable<Scope> GetByClientPublicId(string clientPublicId)
        {
            return ((DaOAuthContext)Context).Scopes.
                Where(c => c.Client.PublicId.Equals(clientPublicId));
        }

        public void Update(Scope toUpdate)
        {
            ((DbContext)Context).Set<Scope>().Attach(toUpdate);
            ((DbContext)Context).Entry(toUpdate).State = EntityState.Modified;
        }
    }
}
