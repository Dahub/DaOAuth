using DaOAuth.Domain;
using System.Collections.Generic;

namespace DaOAuth.Dal.Interface
{
    public interface ICodeRepository : IRepository
    {
        void Add(Code toAdd);
        IEnumerable<Code> GetAllByClientId(string clientPublicId);
        void Update(Code toUpdate);
        void Delete(Code toDelete);
    }
}
