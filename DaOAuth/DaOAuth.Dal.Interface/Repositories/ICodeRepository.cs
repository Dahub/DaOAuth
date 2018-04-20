using DaOAuth.Domain;

namespace DaOAuth.Dal.Interface
{
    public interface ICodeRepository : IRepository
    {
        void Add(Code toAdd);
    }
}
