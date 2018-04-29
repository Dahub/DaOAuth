using System;
using System.ComponentModel.DataAnnotations;

namespace DaOAuth.Service
{
    public class UserDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        [MaxLength(32, ErrorMessage = "Le nom d'utilisateur ne doit pas excéder 32 caractères")]
        public string UserName { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
