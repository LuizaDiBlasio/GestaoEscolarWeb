using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class RegisterNewUserViewModel : User
    {

        [Required]
        [DataType(DataType.EmailAddress)] // Obriga a colocar email
        public string Username { get; set; }


        [Required(ErrorMessage = "You need to select a role for your user")]
        [Display(Name = "User role")]
        public string SelectedRole { get; set; }


        [RequiredIfRoleIsStudent]
        [Display(Name = "Profile picture")]
        public IFormFile ImageFile { get; set; }


        public IEnumerable<SelectListItem> AvailableRoles { get; set; }
    }
}
