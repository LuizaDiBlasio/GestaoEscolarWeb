using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Display(Name = "Image")]
        public Guid? ImageId { get; set; }


        //propriedade de leitura
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
