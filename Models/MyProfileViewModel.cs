using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class MyProfileViewModel
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] //mensagem não chega a ser mostrada
        public string FullName { get; set; }



        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }


        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }


        [Required]
        public string Address { get; set; }


        public string Email { get; set; }


        public Guid ProfileImageId { get; set; }


        [Display(Name = "Profile Image")]
        public IFormFile ImageFile { get; set; }


        public string ImageFullPath => ProfileImageId == Guid.Empty
              ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
    : $"https://gestaoescolar.blob.core.windows.net/imagens/{ProfileImageId}";
    }
}
