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


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }   


        [Display(Name = "User Image")]
        public Guid? ImageId { get; set; }


        public bool IsActive { get; set; } = true; //para indicar quando o user é deletado


        public string ImageFullPath => ImageId == null || ImageId == Guid.Empty
     ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
     : $"https://gestaoescolar.blob.core.windows.net/imagens/{ImageId}";

    }
}
