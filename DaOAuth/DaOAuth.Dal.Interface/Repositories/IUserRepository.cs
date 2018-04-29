using DaOAuth.Domain;

namespace DaOAuth.Dal.Interface
{
    public interface IUserRepository : IRepository
    {
        void Add(User toAdd);
        User GetByUserName(string userName);
        void Update(User toUpdate);
    }
}
