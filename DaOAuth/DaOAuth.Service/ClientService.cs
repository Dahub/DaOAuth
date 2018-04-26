using DaOAuth.Domain;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DaOAuth.Service
{
    public class ClientService : ServiceBase
    {
        public bool AreClientCredentialsValid(string basicAuthCredentials)
        {
            bool toReturn = false;

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

                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        var toCompare = sha1.ComputeHash(Encoding.UTF8.GetBytes(clientSecret));
                        toReturn = toCompare.SequenceEqual(client.ClientSecret);
                    }
                }
            }

            return toReturn;
        }

        public bool IsCodeValidForAuthorizationCodeGrant(string clientPublicId, string code)
        {
            if (String.IsNullOrEmpty(clientPublicId))
                return false;

            if (String.IsNullOrEmpty(code))
                return false;

            if (!CheckIfCodeIsValid(clientPublicId, code))
                return false;

            return true;
        }

        public bool IsClientValidForAuthorizationCodeGrant(string clientPublicId, string requestRedirectUri)
        {
            try
            {
                if (String.IsNullOrEmpty(clientPublicId))
                    return false;

                if (String.IsNullOrEmpty(requestRedirectUri))
                    return false;

                if (!CheckIfClientIsValid(clientPublicId, requestRedirectUri))
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
                        ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds()
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

        private bool CheckIfClientIsValid(string clientPublicId, string requestRedirectUri)
        {
            using (var context = Factory.CreateContext(ConnexionString))
            {
                var clientRepo = Factory.GetClientRepository(context);
                var client = clientRepo.GetByPublicId(clientPublicId);

                if (client == null)
                    return false;

                if (!client.IsValid)
                    return false;

                if (client.ClientTypeId != (int)EClientType.CONFIDENTIAL)
                    return false;

                if (client.DefautRedirectUri != requestRedirectUri)
                    return false;
            }

            return true;
        }
    }
}
