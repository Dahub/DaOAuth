using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public interface IUserClientService
    {
        IEnumerable<UserClientDto> GetUserClientsByUserName(string userName);
        void RevokeClient(string clientId, string username);
        void ChangeAuthorizationClient(string clientId, bool authorize, string username);
    }
}
