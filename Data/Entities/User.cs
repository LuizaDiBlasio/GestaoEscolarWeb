using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class User : IdentityUser
    {

        [MaxLength(50, ErrorMessage = "The field {0} can only contain {1} characters length")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }


        [MaxLength(100, ErrorMessage = "The field {0} can only contain {1} characters length")]
        public string Address { get; set; }


        [Display(Name = "User Image")]
        public Guid? ImageId { get; set; }


        public string ImageFullPath => ImageId == Guid.Empty
              ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
    : $"https://gestaoescolar.blob.core.windows.net/imagens/{ImageId}";

    }
}
