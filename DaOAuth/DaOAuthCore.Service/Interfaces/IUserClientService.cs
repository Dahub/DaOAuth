using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public interface IUserClientService
    {
        IEnumerable<UserClientDto> GetUserClientsByUserName(string userName);
        void RevokeClient(string client_id, string username);
        void ChangeAuthorizationClient(string client_id, bool authorize, string username);
    }
}
