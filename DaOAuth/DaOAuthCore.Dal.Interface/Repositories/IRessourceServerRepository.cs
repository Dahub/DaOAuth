using DaOAuthCore.Domain;

namespace DaOAuthCore.Dal.Interface
{
    public interface IRessourceServerRepository : IRepository
    {
        RessourceServer GetByLogin(string login);
    }
}
