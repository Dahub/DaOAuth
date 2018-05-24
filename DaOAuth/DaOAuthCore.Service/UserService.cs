using DaOAuthCore.Domain;
using System;

namespace DaOAuthCore.Service
{
    public class UserService : ServiceBase, IUserService
    {
        public UserDto Find(string userName, string password)
        {
            UserDto toReturn = null;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var userRepo = Factory.GetUserRepository(context);

                    var user = userRepo.GetByUserName(userName);
                    if (user != null && AreEqualsSha1(
                        String.Concat(Configuration.PasswordSalt, password), user.Password))
                    {
                        toReturn = user.ToDto();
                    }
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la récupération de l'utilisateur {0}", userName), ex);
            }

            return toReturn;
        }

        public bool CheckIfUserExist(string userName)
        {
            bool toReturn = false;

            try
            {
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var userRepo = Factory.GetUserRepository(context);
                    toReturn = userRepo.GetByUserName(userName) != null;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la vérification de l'existance de l'utilisateur {0}", userName), ex);
            }

            return toReturn;
        }

        public UserDto CreateUser(UserDto toCreate, string password)
        {
            try
            {
                if (String.IsNullOrEmpty(password))
                    throw new DaOauthServiceException("Le mot de passe n'est pas renseigné");

                // vérification que l'user name n'existe pas déjà
                using (var context = Factory.CreateContext(ConnexionString))
                {
                    var userRepo = Factory.GetUserRepository(context);
                    if (userRepo.GetByUserName(toCreate.UserName) != null)
                        throw new DaOauthServiceException(String.Format("Le nom d'utilisateur {0} est déjà utilisé", toCreate.UserName));

                    // initialisation de l'utilisateur
                    User myUser = new User()
                    {
                        BirthDate = toCreate.BirthDate,
                        CreationDate = DateTime.Now,
                        FullName = toCreate.FullName,
                        IsValid = true,
                        Password = Sha1Hash(String.Concat(Configuration.PasswordSalt, password)),
                        UserName = toCreate.UserName
                    };

                    userRepo.Add(myUser);

                    context.Commit();

                    toCreate.Id = myUser.Id;
                }
            }
            catch (DaOauthServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DaOauthServiceException(String.Format("Erreur lors de la création d'un nouvel utilisateur {0}", toCreate.UserName), ex);
            }

            return toCreate;
        }
    }
}
