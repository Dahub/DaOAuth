using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaOAuthCore.Dal.EF
{
    internal class RessourceServerRepository : IRessourceServerRepository
    {
        public IContext Context { get; set; }

        public RessourceServer GetByLogin(string login)
        {
            return ((DaOAuthContext)Context).RessourceServers.
               Where(rs => rs.Login.Equals(login, StringComparison.Ordinal)).FirstOrDefault();
        }

        public IEnumerable<RessourceServer> GetAllActives()
        {
            return ((DaOAuthContext)Context).RessourceServers.
                Where(rs => rs.IsValid.Equals(true));
        }
    }
}
