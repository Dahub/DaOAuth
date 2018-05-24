using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using System;
using System.Linq;

namespace DaOAuthCore.Dal.EF
{
    internal class RessourceServerRepository : IRessourceServerRepository
    {
        public IContext Context { get; set; }

        public RessourceServer GetByLogin(string login)
        {
            return ((DaOAuthContext)Context).RessourceServers.
               Where(c => c.Login.Equals(login, StringComparison.Ordinal)).FirstOrDefault();
        }
    }
}
