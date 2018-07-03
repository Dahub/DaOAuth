using System;
using DaOAuthCore.Domain;

namespace DaOAuthCore.Dal.Interface
{
    public interface IUserRepository : IRepository
    {
        void Add(User toAdd);
        User GetByUserName(string userName);
        void Update(User toUpdate);
        User GetUserByUserPublicIdAndClientId(Guid value, string clientId);
    }
}
