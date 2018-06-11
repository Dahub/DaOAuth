using DaOAuthCore.Domain;
using System.Collections.Generic;

namespace DaOAuthCore.Dal.Interface
{
    public interface IRessourceServerRepository : IRepository
    {
        RessourceServer GetByLogin(string login);
        IEnumerable<RessourceServer> GetAllActives();
    }
}
