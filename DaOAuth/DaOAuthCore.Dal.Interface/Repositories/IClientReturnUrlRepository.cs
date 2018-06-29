using DaOAuthCore.Domain;
using System.Collections.Generic;

namespace DaOAuthCore.Dal.Interface
{
    public interface IClientReturnUrlRepository : IRepository
    {
        void Add(ClientReturnUrl toAdd);
        IEnumerable<ClientReturnUrl> GetAllByClientId(string clientPublicId);
    }
}
