using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class RegisterNewUserViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }


        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }


        [Required]
        [DataType(DataType.EmailAddress)] // Obriga a colocar email
        public string Username { get; set; }


        [MaxLength(100, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string Address { get; set; }


        [MaxLength(20, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string PhoneNumber { get; set; }


        [Required]
        [MinLength(6)]
        public string Password { get; set; }


        [Required]
        [Compare("Password")] //confirma se campo abaixo é igual ao campo de cima.
        public string Confirm { get; set; }


        [Required(ErrorMessage = "You need to select a role for your user")]
        [Display(Name = "User role")]
        public string SelectedRole { get; set; }


        public IEnumerable<SelectListItem> AvailableRoles { get; set; }
    }
}
