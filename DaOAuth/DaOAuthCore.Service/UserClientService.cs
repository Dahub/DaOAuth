using System;
using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public class UserClientService : ServiceBase, IUserClientService
    {
        public IEnumerable<UserClientDto> GetUserClientsByUserName(string userName)
        {
            IList<UserClientDto> toReturn = new List<UserClientDto>();

            try
            {
                using (var context = Factory.CreateContext(Configuration.DaOAuthConnexionString))
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
                throw new DaOauthServiceException(String.Format("Erreur lors de la récupération des clients de l'utilisateur {0}", userName), ex);
            }

            return toReturn;
        }
    }
}
