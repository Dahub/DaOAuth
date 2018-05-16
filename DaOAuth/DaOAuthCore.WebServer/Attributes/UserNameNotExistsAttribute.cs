using DaOAuthCore.Dal.EF;
using DaOAuthCore.Service;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuthCore.WebServer
{
    public class UserNameNotExistsAttribute : ValidationAttribute
    {
        public AppConfiguration AppConfig { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (String.IsNullOrEmpty(value.ToString()))
                return false;

            var serv = new UserService()
            {
                Configuration = AppConfig,
                Factory = new EfRepositoriesFactory()
            };

            return !serv.CheckIfUserExist(value.ToString());
        }
    }
}