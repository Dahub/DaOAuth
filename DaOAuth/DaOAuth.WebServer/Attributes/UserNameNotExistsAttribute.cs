using DaOAuth.Dal.EF;
using DaOAuth.Service;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuth.WebServer
{
    public class UserNameNotExistsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (String.IsNullOrEmpty(value.ToString()))
                return false;

            var serv = new UserService()
            {
                ConnexionString = ConfigurationWrapper.Instance.ConnexionString,
                Factory = new EfRepositoriesFactory()
            };

            return !serv.CheckIfUserExist(value.ToString());
        }
    }
}