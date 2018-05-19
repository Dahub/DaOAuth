using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuthCore.WebServer.Models
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(32, ErrorMessage = "Le nom d'utilisateur ne doit pas excéder 32 caractères")]
        [Display(Name = "Nom d'utilisateur")]
        [UserNameNotExists]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les deux mots de passe ne sont pas identiques")]
        [Display(Name = "Confirmez le mot de passe")]
        public string PassWordConfirmation { get; set; }

        [Display(Name = "Nom complet")]
        public string FullName { get; set; }

        [Display(Name = "Date de naissance")]
        public DateTime? BirthDate { get; set; }
    }
}