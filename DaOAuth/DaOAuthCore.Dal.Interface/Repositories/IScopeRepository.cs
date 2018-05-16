using DaOAuthCore.Domain;
using System.Collections.Generic;

namespace DaOAuthCore.Dal.Interface
{
    public interface IScopeRepository : IRepository
    {
        void Delete(Scope toDelete);
        void Update(Scope toUpdate);
        void Add(Scope toAdd);
        IEnumerable<Scope> GetByClientPublicId(string clientPublicId);        
    }
}
