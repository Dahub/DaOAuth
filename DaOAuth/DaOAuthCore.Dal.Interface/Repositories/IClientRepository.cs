using DaOAuthCore.Domain;
using System.Collections.Generic;

namespace DaOAuthCore.Dal.Interface
{
    public interface IClientRepository : IRepository
    {
        void Add(Client toAdd);
        Client GetByPublicId(string publicId);
        void Update(Client toUpdate);
        IEnumerable<Client> GetAllByUserName(string userName);
    }
}
