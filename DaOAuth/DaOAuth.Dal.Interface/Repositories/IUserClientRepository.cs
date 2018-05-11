using DaOAuth.Domain;
using System.Collections.Generic;

namespace DaOAuth.Dal.Interface
{
    public interface IUserClientRepository : IRepository
    {
        UserClient GetUserClientByUserNameAndClientPublicId(string clientPublicId, string userName);
        IEnumerable<UserClient> GetAllByUserName(string userName);
        void Add(UserClient userClient);
        void Delete(UserClient userClient);
        void Update(UserClient userClient);
    }
}
