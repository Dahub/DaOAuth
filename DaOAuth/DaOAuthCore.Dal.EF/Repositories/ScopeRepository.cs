using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaOAuthCore.Dal.EF
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
            return ((DaOAuthContext)Context).ClientsScopes.
              Where(c => c.Client.PublicId.Equals(clientPublicId, StringComparison.Ordinal)).Select(c => c.Scope);
        }

        public void Update(Scope toUpdate)
        {
            ((DbContext)Context).Set<Scope>().Attach(toUpdate);
            ((DbContext)Context).Entry(toUpdate).State = EntityState.Modified;
        }
    }
}
