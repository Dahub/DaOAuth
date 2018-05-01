using DaOAuth.Domain;
using System;
using System.Linq;
using System.Text;

namespace DaOAuth.Service
{
    public class ClientService : ServiceBase
    {
        public bool IsClientAuthoryzeByUser(string publicId, string userName)
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
                throw new DaOauthServiceException(String.Format("Erreur lors de la vérification de l'authorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
            }

            return toReturn;
        }

        public void AuthorizeClientForUser(string publicId, string userName)
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
                        UserPublicId = RandomMaker.GenerateRandomInt(8)
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

        public void RevokeClientForUser(string publicId, string userName)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(publicId, userName);

                    if (clientUser == null)
                        throw new DaOauthServiceException("Impossible de révoquer, authorisation introuvable");

                    clientUserRepo.Delete(clientUser);

                    context.Commit();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la révocation de l'authorisation de l'utilisateur {0} avec le client {1}", userName, publicId), ex);
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

                        return AreEqualsSha1(clientSecret, client.ClientSecret);
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

        public void UpdateRefreshTokenForClient(string refreshToken, string clientPublicId)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    var client = clientRepo.GetByPublicId(clientPublicId);

                    client.RefreshToken = refreshToken;

                    clientRepo.Update(client);

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

        public Code GenerateAndAddCodeToClient(string clientPublicId)
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
                        CodeValue = RandomMaker.GenerateRandomString(24),
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
