using DaOAuth.Domain;

namespace DaOAuth.Dal.Interface
{
    public interface IClientRepository : IRepository
    {
        void Add(Client toAdd);
        Client GetByPublicId(string publicId);
    }
}
