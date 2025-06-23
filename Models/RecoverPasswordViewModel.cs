using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class RecoverPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
