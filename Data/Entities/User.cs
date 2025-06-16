using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(50, ErrorMessage = "The field {0} can only contain {1} characters length")]
        public string FirstName { get; set; }


        [MaxLength(50, ErrorMessage = "The field {0} can only contain {1} characters length")]
        public string LastName { get; set; }


        [MaxLength(100, ErrorMessage = "The field {0} can only contain {1} characters length")]
        public string Address { get; set; }


        [Display(Name = "Image")]
        public Guid? ImageId { get; set; }


        //propriedade de leitura
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
