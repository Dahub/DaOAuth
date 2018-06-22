using System;
using System.Collections.Generic;
using System.Globalization;

namespace DaOAuthCore.Service
{
    public class UserClientService : ServiceBase, IUserClientService
    {
        public IEnumerable<UserClientDto> GetUserClientsByUserName(string userName)
        {
            IList<UserClientDto> toReturn = new List<UserClientDto>();

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientRepo = Factory.GetUserClientRepository(context);
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

        public void RevokeClient(string clientId, string username)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(clientId, username);
                    if (clientUser != null)
                    {
                        clientUserRepo.Delete(clientUser);
                        context.Commit();
                    }
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors de la révocation du client", ex);
            }
        }

        public void ChangeAuthorizationClient(string clientId, bool authorize, string username)
        {
            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var clientUserRepo = Factory.GetUserClientRepository(context);
                    var clientUser = clientUserRepo.GetUserClientByUserNameAndClientPublicId(clientId, username);
                    if (clientUser != null)
                    {
                        clientUser.IsValid = authorize;
                        clientUserRepo.Update(clientUser);
                        context.Commit();
                    }
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException("Erreur lors du changement d'authorisation du client", ex);
            }
        }
    }
}
