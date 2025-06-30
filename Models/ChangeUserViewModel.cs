using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }


        [MaxLength(100, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string Address { get; set; }


        [MaxLength(20, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string PhoneNumber { get; set; }


    }
}
