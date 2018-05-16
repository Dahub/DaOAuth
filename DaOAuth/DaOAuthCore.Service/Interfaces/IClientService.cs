using System;
using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public interface IClientService
    {
        IEnumerable<ClientDto> GetClientsByUserName(string userName);
        ClientDto GetClientByPublicId(string publicId);
        bool HasUserAuthorizeOrDeniedClientAccess(string publicId, string userName);
        bool IsClientAuthorizeByUser(string publicId, string userName, out Guid userPublicId);
        bool AreScopesAuthorizedForClient(string client_id, string scope);
        void AuthorizeOrDeniedClientForUser(string publicId, string userName, bool authorize);
        string GetClientIdFromAuthorizationHeaderValue(string headerValue);
        bool AreClientCredentialsValid(string basicAuthCredentials);
        CodeDto GetCodeInfos(string clientPublicId, string code);
        bool IsClientValidForAuthorization(string clientPublicId, string requestRedirectUri, string responseType);
        bool IsRefreshTokenValid(string userName, string clientPublicId, string refresh_token);
        void UpdateRefreshTokenForClient(string refreshToken, string clientPublicId, string userName);
        ClientDto CreateNewClient(string name, string defaulRedirectUrl);
        string GenerateAndAddCodeToClient(string clientPublicId, string userName, string scope, Guid userPublicId);
        Guid GetUserPublicId(string client_id, string username);
    }
}