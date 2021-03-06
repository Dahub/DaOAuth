﻿using DaOAuthCore.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DaOAuthCore.Service
{
    public class ClientService : ServiceBase, IClientService
    {
        public IEnumerable<ClientDto> GetClientsByUserName(string userName)
        {
            IList<ClientDto> toReturn = new List<ClientDto>();

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    toReturn = clientRepo.GetAllByUserName(userName).ToDto();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la récupération des clients de l'utilisateur {0}", userName), ex);
            }

            return toReturn;
        }

        public ClientDto GetClientByPublicId(string publicId)
        {
            ClientDto toReturn = null;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    var client = clientRepo.GetByPublicId(publicId);
                    if (client == null || !client.IsValid)
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Client {0} introuvable ou invalide", publicId));

                    toReturn = client.ToDto();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la récupération du client {0}", publicId), ex);
            }

            return toReturn;
        }

        public bool HasUserAuthorizeOrDeniedClientAccess(string publicId, string userName)
        {
            bool toReturn = false;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    toReturn = clientUserRepo.GetUserClientByUserNameAndClientPublicId(publicId, userName) != null;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la vérification de l'autorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }

            return toReturn;
        }

        public bool IsClientAuthorizeByUser(string publicId, string userName, out Guid userPublicId)
        {
            bool toReturn = false;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(publicId, userName);
                    userPublicId = clientUser.UserPublicId;
                    toReturn = clientUser != null && clientUser.IsValid;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la vérification de l'autorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }

            return toReturn;
        }

        public bool AreScopesAuthorizedForClient(string clientId, string scope)
        {
            try
            {
                string[] scopes = null;
                if (!String.IsNullOrEmpty(scope))
                    scopes = scope.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var scopeRepo = Factory.GetScopeRepository(context);

                    IEnumerable<string> clientScopes = scopeRepo.GetByClientPublicId(clientId).Select(s => s.Wording.ToUpper(CultureInfo.CurrentCulture));

                    if ((scopes == null || scopes.Length == 0) && clientScopes.Count() == 0) // client sans scope défini
                        return true;

                    if ((scopes == null || scopes.Length == 0) && clientScopes.Count() > 0) // client avec scopes définis
                        return false;

                    foreach (var s in scopes)
                    {
                        if (!clientScopes.Contains<string>(s.ToUpper(CultureInfo.CurrentCulture)))
                            return false;
                    }
                }

                return true;
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la vérification de l'autorisation des scopes du client {0}", clientId), ex);
            }
        }

        public void AuthorizeOrDeniedClientForUser(string publicId, string userName, bool authorize)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    var userRepo = Factory.GetUserRepository(context);
                    var clientUserRepo = Factory.GetUserClientRepository(context);

                    // on vérifie que le client n'existe pas déjà
                    if (clientUserRepo.GetUserClientByUserNameAndClientPublicId(publicId, userName) != null)
                        return;

                    var client = clientRepo.GetByPublicId(publicId);
                    if (client == null || !client.IsValid)
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Client {0} introuvable ou invalide", publicId));

                    var user = userRepo.GetByUserName(userName);
                    if (user == null || !user.IsValid)
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Utilisateur {0} introuvable ou invalide", userName));

                    clientUserRepo.Add(new UserClient()
                    {
                        ClientId = client.Id,
                        CreationDate = DateTime.Now,
                        UserId = user.Id,
                        IsValid = authorize,
                        UserPublicId = Guid.NewGuid()
                    });

                    context.Commit();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de l'authorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }
        }

        public string GetClientIdFromAuthorizationHeaderValue(string headerValue)
        {
            string toReturn = String.Empty;

            try
            {
                string[] authsInfos = headerValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (authsInfos.Length != 2)
                    throw new DaOauthServiceException("Extraction d'id du client : valeur de header incorrecte");

                if (!authsInfos[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
                    throw new DaOauthServiceException("Extraction d'id du client : le header d'authentification doit être de type Basic");

                string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authsInfos[1]));
                int separatorIndex = credentials.IndexOf(':');
                if (separatorIndex >= 0)
                {
                    toReturn = credentials.Substring(0, separatorIndex);
                }

                if (String.IsNullOrEmpty(toReturn))
                    throw new DaOauthServiceException("Extraction d'id du client : impossible de trouver l'id");
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de l'extraction du client_id à partir de la valeur du header d'authorization", ex);
            }

            return toReturn;
        }

        public bool AreClientCredentialsValid(string basicAuthCredentials)
        {
            bool toReturn = false;

            try
            {
                string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(basicAuthCredentials));
                int separatorIndex = credentials.IndexOf(':');
                if (separatorIndex >= 0)
                {
                    string clientPublicId = credentials.Substring(0, separatorIndex);
                    string clientSecret = credentials.Substring(separatorIndex + 1);

                    using (var context = Factory.CreateContext(ConnexionString))
                    {
                        var clientRepo = Factory.GetClientRepository(context);
                        var client = clientRepo.GetByPublicId(clientPublicId);

                        if(client != null)
                            toReturn = AreEqualsSha1(clientSecret, client.ClientSecret) && client.IsValid;
                    }
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la vérification des credentials", ex);
            }

            return toReturn;
        }

        public CodeDto GetCodeInfos(string clientPublicId, string code)
        {
            CodeDto toReturn = new CodeDto()
            {
                IsValid = false
            };

            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return toReturn;

                if (String.IsNullOrEmpty(code))
                    return toReturn;

                if (!CheckIfCodeIsValid(clientPublicId, code, out Code myCode))
                    return toReturn;

                toReturn.IsValid = true;
                toReturn.Scope = myCode.Scope;
                toReturn.UserName = myCode.UserName;
                toReturn.UserPublicId = myCode.UserPublicId;

                return toReturn;
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la vérification de la validité du code client", ex);
            }
        }

        public bool IsClientValidForAuthorization(string clientPublicId, Uri requestRedirectUri, string responseType)
        {
            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return false;

                if (requestRedirectUri == null)
                    return false;

                switch (responseType)
                {
                    case "code":
                        if (!CheckIfClientIsValid(clientPublicId, requestRedirectUri, EClientType.CONFIDENTIAL))
                            return false;
                        break;
                    case "token":
                        if (!CheckIfClientIsValid(clientPublicId, requestRedirectUri, EClientType.PUBLIC))
                            return false;
                        break;
                    default:
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "response_type non pris en charge : {0}", responseType));
                }

                return true;
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la tentative de vérification de validité du client {0}", clientPublicId), ex);
            }
        }

        public bool IsRefreshTokenValid(string userName, string clientPublicId, string refreshToken)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var client = clientUserRepo.GetUserClientByUserNameAndClientPublicId(clientPublicId, userName);

                    if (client == null || !client.IsValid)
                        return false;

                    return client.RefreshToken != null && client.RefreshToken.Equals(refreshToken, StringComparison.Ordinal);
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la vérification de la validité du refresh token", ex);
            }
        }

        public void UpdateRefreshTokenForClient(string refreshToken, string clientPublicId, string userName)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var client = clientUserRepo.GetUserClientByUserNameAndClientPublicId(clientPublicId, userName);

                    client.RefreshToken = refreshToken;

                    clientUserRepo.Update(client);

                    context.Commit();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la mise à jour du refresh token du client {0}", clientPublicId), ex);
            }
        }

        public ClientDto CreateNewClient(string name, string defaulRedirectUrl)
        {
            Client result = null;

            try
            {
                if (String.IsNullOrEmpty(name))
                    throw new DaOauthServiceException("Le nom du client est obligatoire");

                if (!String.IsNullOrEmpty(defaulRedirectUrl))
                {
                    if (!Uri.TryCreate(defaulRedirectUrl, UriKind.Absolute, out Uri u))
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "L'url de redirection {0} n'est pas valide", defaulRedirectUrl));
                }

                result = new Client()
                {
                    IsValid = true,
                    CreationDate = DateTime.Now,
                    Name = name,                    
                    PublicId = RandomMaker.GenerateRandomString(16)
                };

                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    clientRepo.Add(result);

                    var clientReturnUrlRepo = Factory.GetClientReturnUrlRepository(context);
                    clientReturnUrlRepo.Add(new ClientReturnUrl()
                    {
                        ClientId = result.Id,
                        ReturnUrl = defaulRedirectUrl
                    });

                    context.Commit();                    
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la création du client", ex);
            }

            return result.ToDto();
        }

        public string GenerateAndAddCodeToClient(string clientPublicId, string userName, string scope, Guid userPublicId)
        {
            string toReturn = null;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    // récupération du client
                    var clientRepo = Factory.GetClientRepository(context);
                    Client myClient = clientRepo.GetByPublicId(clientPublicId);
                    if (myClient == null || !myClient.IsValid)
                        throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Client {0} invalide", clientPublicId));

                    var codeRepo = Factory.GetCodeRepository(context);

                    // suppression des codes invalides
                    long now = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                    foreach (var c in codeRepo.GetAllByClientId(clientPublicId))
                    {
                        if (!c.IsValid || c.ExpirationTimeStamp < now)
                            codeRepo.Delete(c);
                    }

                    // création d'un nouveau code
                    Code myCode = new Code()
                    {
                        ClientId = myClient.Id,
                        CodeValue = RandomMaker.GenerateRandomString(24), // StringCipher.Encrypt(String.Format(CODE_PATTERN, RandomMaker.GenerateRandomString(6), userName, RandomMaker.GenerateRandomString(6)), ConfigurationWrapper.Instance.PasswordForStringCypher),
                        IsValid = true,
                        Scope = scope,
                        UserName = userName,
                        UserPublicId = userPublicId,
                        ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(2)).ToUnixTimeSeconds()
                    };
                    codeRepo.Add(myCode);

                    context.Commit();

                    toReturn = myCode.CodeValue;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format(CultureInfo.InvariantCulture, "Erreur lors de la création d'un nouveau code pour le client {0}", clientPublicId), ex);
            }

            return toReturn;
        }

        public Guid? GetUserPublicId(string clientId, string username)
        {
            Guid? toReturn = null;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(clientId, username);

                    if (clientUser != null)
                        toReturn = clientUser.UserPublicId;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la récupération de l'id publique de l'utilisateur", ex);
            }

            return toReturn;
        }
      
        private bool CheckIfCodeIsValid(string clientPublicId, string code, out Code myCode)
        {
            using (var context = Factory.CreateContext(ConnexionString))
            {
                var codeRepo = Factory.GetCodeRepository(context);
                var codes = codeRepo.GetAllByClientId(clientPublicId);

                myCode = null;

                if (codes == null || codes.Count() == 0)
                    return false;

                myCode = codes.Where(c => c.CodeValue.Equals(code, StringComparison.Ordinal)).FirstOrDefault();
                if (myCode == null)
                    return false;

                if (!myCode.IsValid)
                    return false;

                if (myCode.ExpirationTimeStamp < new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
                    return false;

                // on en profite pour rendre le code invalide : 1 seule demande possible
                myCode.IsValid = false;

                codeRepo.Update(myCode);

                context.Commit();
            }

            return true;
        }

        private bool CheckIfClientIsValid(string clientPublicId, Uri requestRedirectUri, EClientType clientType)
        {
            using (var context = Factory.CreateContext(ConnexionString))
            {
                var clientRepo = Factory.GetClientRepository(context);
                var clientReturnUrlRepo = Factory.GetClientReturnUrlRepository(context);

                var client = clientRepo.GetByPublicId(clientPublicId);

                if (client == null)
                    return false;

                if (!client.IsValid)
                    return false;

                if (client.ClientTypeId != (int)clientType)
                    return false;

                IList<Uri> clientUris = new List<Uri>();
                foreach(var uri in clientReturnUrlRepo.GetAllByClientId(clientPublicId))
                {
                    clientUris.Add(new Uri(uri.ReturnUrl, UriKind.Absolute));
                }

                if (!clientUris.Contains(requestRedirectUri))
                    return false;
            }

            return true;
        }
    }
}
