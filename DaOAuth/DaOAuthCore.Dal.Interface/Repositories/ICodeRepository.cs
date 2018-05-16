using DaOAuthCore.Domain;
using System.Collections.Generic;

namespace DaOAuthCore.Dal.Interface
{
    public interface ICodeRepository : IRepository
    {
        void Add(Code toAdd);
        IEnumerable<Code> GetAllByClientId(string clientPublicId);
        void Update(Code toUpdate);
        void Delete(Code toDelete);
    }
}
