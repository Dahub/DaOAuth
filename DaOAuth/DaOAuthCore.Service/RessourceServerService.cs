using System;
using System.Text;
using System.Linq;

namespace DaOAuthCore.Service
{
    public class RessourceServerService : ServiceBase, IRessourceServerService
    {
        public bool AreRessourceServerCredentialsValid(string basicAuthCredentials)
        {
            bool toReturn = false;

            try
            {
                string credentials = Encoding.UTF8.GetString(Convert.FromBase64String(basicAuthCredentials));
                int separatorIndex = credentials.IndexOf(':');
                if (separatorIndex >= 0)
                {
                    string login = credentials.Substring(0, separatorIndex);
                    string serverSecret = credentials.Substring(separatorIndex + 1);

                    using (var context = Factory.CreateContext(ConnexionString))
                    {
                        var rsRepo = Factory.GetRessourceServerRepository(context);
                        var rs = rsRepo.GetByLogin(login);

                        if (rs == null)
                            return false;

                        return AreEqualsSha1(serverSecret, rs.ServerSecret);
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

        public string[] GetAllRessourcesServersNames()
        {
            string[] result = Array.Empty<string>();

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var rsRepo = Factory.GetRessourceServerRepository(context);
                    result = rsRepo.GetAllActives().Select(rs => rs.Name).ToArray();
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la récupération des noms des serveurs de ressources", ex);
            }

            return result;
        }
    }
}
