using GestaoEscolarWeb.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class ChangeUserViewModel
    {
        [Display(Name = "Username")]
        [Required]
        public string SearchUserName { get; set; }

    
        [Display(Name = "Full Name")]
        public string FullName { get; set; }


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }


        [MaxLength(100, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string Address { get; set; }


        [MaxLength(20, ErrorMessage = "The field {0} only can cointain {1} characters.")]
        public string PhoneNumber { get; set; }

        public string Email { get; set; }


        public Guid ProfileImageId { get; set; }


        [Display(Name = "Profile Image")]
        [MaxFileSize(5 * 1024 * 1024)]
        [RequiredIfRoleIsStudent]
        public IFormFile ImageFile { get; set; }


        public string ImageFullPath => ProfileImageId == Guid.Empty
              ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação
    : $"https://gestaoescolar.blob.core.windows.net/imagens/{ProfileImageId}";

        public bool IsSearchSuccessful { get; set; }   
    }
}
