using DaOAuth.Domain;
using System;

namespace DaOAuth.Service
{
    public class ClientService : ServiceBase
    {
        public ClientInfoForAuthorizationCodeGrant GetClientInfoForAuthorizationCodeGrant(string clientPublicId, string requestRedirectUri)
        {
            ClientInfoForAuthorizationCodeGrant toReturn = new ClientInfoForAuthorizationCodeGrant()
            {
                IsValid = false
            };

            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return toReturn;

                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetClientRepository(context);
                    var client = clientRepo.GetByPublicId(clientPublicId);

                    if (client == null)
                        return toReturn;

                    if (!client.IsValid)
                        return toReturn;

                    if (client.ClientTypeId != (int)EClientType.CONFIDENTIAL)
                        return toReturn;

                    toReturn.IsValid = true;
                    toReturn.RedirectUri = String.IsNullOrEmpty(requestRedirectUri)?client.DefautRedirectUri:requestRedirectUri;
                }

                return toReturn;
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

        public Code AddCodeToClient(string clientPublicId)
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

                    // création d'un nouveau code
                    toReturn = new Code()
                    {
                        ClientId = myClient.Id,
                        CodeValue = RandomMaker.GenerateRandomString(24),
                        IsValid = true,
                        ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds()
                    };

                    var codeRepo = Factory.GetCodeRepository(context);
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
    }
}
