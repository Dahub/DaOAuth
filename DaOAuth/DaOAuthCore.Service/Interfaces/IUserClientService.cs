using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public interface IUserClientService
    {
        IEnumerable<UserClientDto> GetUserClientsByUserName(string userName);
    }
}
