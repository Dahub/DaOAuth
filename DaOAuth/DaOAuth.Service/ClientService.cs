﻿using DaOAuth.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DaOAuth.Service
{
    public class ClientService : ServiceBase
    {
        private const string CODE_PATTERN = "{0}#;#{1}#;#{2}";

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
                throw new DaOauthServiceException(String.Format("Erreur lors de la récupération des clients de l'utilisateur {0}", userName), ex);
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
                        throw new DaOauthServiceException(String.Format("Client {0} introuvable ou invalide", publicId));

                    toReturn = client.ToDto();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la récupération du client {0}", publicId), ex);
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
                throw new DaOauthServiceException(String.Format("Erreur lors de la vérification de l'autorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }

            return toReturn;
        }

        public bool IsClientAuthorizeByUser(string publicId, string userName)
        {
            bool toReturn = false;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(publicId, userName);
                    toReturn = clientUser != null && clientUser.IsValid;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la vérification de l'autorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }

            return toReturn;
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

                    var client = clientRepo.GetByPublicId(publicId);
                    if (client == null || !client.IsValid)
                        throw new DaOauthServiceException(String.Format("Client {0} introuvable ou invalide", publicId));

                    var user = userRepo.GetByUserName(userName);
                    if (user == null || !user.IsValid)
                        throw new DaOauthServiceException(String.Format("Utilisateur {0} introuvable ou invalide", userName));

                    clientUserRepo.Add(new UserClient()
                    {
                        ClientId = client.Id,
                        CreationDate = DateTime.Now,
                        UserId = user.Id,
                        UserPublicId = Guid.NewGuid(),
                        IsValid = authorize
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
                throw new DaOauthServiceException(String.Format("Erreur lors de l'authorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }
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

                        return AreEqualsSha1(clientSecret, client.ClientSecret) && client.IsValid;
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

        public bool IsCodeValidForAuthorizationCodeGrant(string clientPublicId, string code)
        {
            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return false;

                if (String.IsNullOrEmpty(code))
                    return false;

                if (!CheckIfCodeIsValid(clientPublicId, code))
                    return false;

                return true;
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

        public bool IsClientValidForAuthorizationCodeGrant(string clientPublicId, string requestRedirectUri)
        {
            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return false;

                if (String.IsNullOrEmpty(requestRedirectUri))
                    return false;

                if (!CheckIfClientIsValid(clientPublicId, requestRedirectUri, EClientType.CONFIDENTIAL))
                    return false;

                return true;
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la tentative de vérification de validité du client {0}", clientPublicId), ex);
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
                throw new DaOauthServiceException(String.Format("Erreur lors de la mise à jour du refresh token du client {0}", clientPublicId), ex);
            }
        }

        public Client CreateNewClient(string name, string defaulRedirectUrl)
        {
            Client result = null;

            try
            {
                if (String.IsNullOrEmpty(name))
                    throw new DaOauthServiceException("Le nom du client est obligatoire");

                if (!String.IsNullOrEmpty(defaulRedirectUrl))
                {
                    Uri u;
                    if (!Uri.TryCreate(defaulRedirectUrl, UriKind.Absolute, out u))
                        throw new DaOauthServiceException(String.Format("L'url de redirection {0} n'est pas valide", defaulRedirectUrl));
                }

                result = new Client()
                {
                    IsValid = true,
                    CreationDate = DateTime.Now,
                    Name = name,
                    DefautRedirectUri = defaulRedirectUrl,
                    PublicId = RandomMaker.GenerateRandomString(16)
                };

                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    clientRepo.Add(result);
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

            return result;
        }

        public Code GenerateAndAddCodeToClient(string clientPublicId, string userName)
        {
            Code toReturn = null;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    // récupération du client
                    var clientRepo = Factory.GetClientRepository(context);
                    Client myClient = clientRepo.GetByPublicId(clientPublicId);
                    if (myClient == null || !myClient.IsValid)
                        throw new DaOauthServiceException(String.Format("Client {0} invalide", clientPublicId));

                    var codeRepo = Factory.GetCodeRepository(context);

                    // suppression des codes invalides
                    long now = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                    foreach (var c in codeRepo.GetAllByClientId(clientPublicId))
                    {
                        if (!c.IsValid || c.ExpirationTimeStamp < now)
                            codeRepo.Delete(c);
                    }

                    // création d'un nouveau code
                    toReturn = new Code()
                    {
                        ClientId = myClient.Id,
                        CodeValue = StringCipher.Encrypt(
                            String.Format(CODE_PATTERN, RandomMaker.GenerateRandomString(6), userName, RandomMaker.GenerateRandomString(6)),
                            ConfigurationWrapper.Instance.PasswordForStringCypher),
                        IsValid = true,
                        ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(2)).ToUnixTimeSeconds()
                    };
                    codeRepo.Add(toReturn);

                    context.Commit();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la création d'un nouveau code pour le client {0}", clientPublicId), ex);
            }

            return toReturn;
        }

        public string ExtractUserNameFromCode(string code)
        {
            string toReturn = String.Empty;

            try
            {
                var decrypted = StringCipher.Decrypt(code, ConfigurationWrapper.Instance.PasswordForStringCypher);

                string[] infos = decrypted.Split("#;#".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (infos.Length != 3)
                    throw new DaOauthServiceException("Format de code incorrect");

                toReturn = infos[1];
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors du décryptage d'un code", ex);
            }

            return toReturn;
        }

        private bool CheckIfCodeIsValid(string clientPublicId, string code)
        {
            using (var context = Factory.CreateContext(ConnexionString))
            {
                var codeRepo = Factory.GetCodeRepository(context);
                var codes = codeRepo.GetAllByClientId(clientPublicId);

                if (codes == null || codes.Count() == 0)
                    return false;

                Code myCode = codes.Where(c => c.CodeValue.Equals(code)).FirstOrDefault();
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

        private bool CheckIfClientIsValid(string clientPublicId, string requestRedirectUri, EClientType clientType)
        {
            using (var context = Factory.CreateContext(ConnexionString))
            {
                var clientRepo = Factory.GetClientRepository(context);
                var client = clientRepo.GetByPublicId(clientPublicId);

                if (client == null)
                    return false;

                if (!client.IsValid)
                    return false;

                if (client.ClientTypeId != (int)clientType)
                    return false;

                if (client.DefautRedirectUri != requestRedirectUri)
                    return false;
            }

            return true;
        }
    }
}
