using DaOAuthCore.Service;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuthCore.WebServer
{
    public class UserNameNotExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("La valeur ne peut être nulle");

            if (String.IsNullOrEmpty(value.ToString()))
                return new ValidationResult("La valeur ne peut être nulle");

            var serv = (IUserService)validationContext.GetService(typeof(IUserService));

            if(serv.CheckIfUserExist(value.ToString()))
                return new ValidationResult("Le nom d'utilisateur est déjà utilisé");

            return ValidationResult.Success;
        }
    }
}