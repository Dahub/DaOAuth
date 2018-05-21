using DaOAuthCore.Service;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuthCore.WebServer
{
    public class UserNameNotExistsAttribute : ValidationAttribute
    {
        // https://andrewlock.net/injecting-services-into-validationattributes-in-asp-net-core/
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || String.IsNullOrEmpty(value.ToString()))
                return new ValidationResult("La valeur ne peut être nulle");

            var serv = (IUserService)validationContext.GetService(typeof(IUserService));
            if(serv.CheckIfUserExist(value.ToString()))
                return new ValidationResult("Le nom d'utilisateur est déjà utilisé");

            return ValidationResult.Success;
        }
    }
}