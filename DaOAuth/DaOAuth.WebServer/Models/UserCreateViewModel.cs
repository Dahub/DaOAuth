using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuth.WebServer.Models
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(32, ErrorMessage = "Le nom d'utilisateur ne doit pas excéder 32 caractères")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PassWordConfirmation { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}