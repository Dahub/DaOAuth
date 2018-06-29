using DaOAuthCore.Dal.Interface;
using DaOAuthCore.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaOAuthCore.Dal.EF
{
    internal class ClientReturnUrlRepository : IClientReturnUrlRepository
    {
        public IContext Context { get; set; }

        public void Add(ClientReturnUrl toAdd)
        {
            ((DbContext)Context).Set<ClientReturnUrl>().Add(toAdd);
        }

        public IEnumerable<ClientReturnUrl> GetAllByClientId(string clientPublicId)
        {
            return ((DaOAuthContext)Context).ClientReturnUrl.
               Where(c => c.Client.PublicId.Equals(clientPublicId, StringComparison.Ordinal));
        }
    }
}
