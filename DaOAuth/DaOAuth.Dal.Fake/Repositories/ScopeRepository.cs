using DaOAuth.Dal.Interface;
using DaOAuth.Domain;
using System;
using System.Collections.Generic;

namespace DaOAuth.Dal.Fake
{
    internal class ScopeRepository : IScopeRepository
    {
        public IContext Context { get; set; }

        public void Add(Scope toAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Scope toDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Scope> GetByClientPublicId(string clientPublicId)
        {
            throw new NotImplementedException();
        }

        public void Update(Scope toUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
