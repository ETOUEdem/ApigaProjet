using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Apim.Data.Repository.Models
{
    public class User
    {
        public int Id { get; set; }

        [Display(Name = "Utilisateur")]
        public string Username { get; set; }

        [Display(Name = "Mot de passe")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirmation de mot de passe")]
        [DataType(DataType.Password)]
        [Compare("Password")]
     
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
    }
}
