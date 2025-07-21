using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;
using GestaoEscolarWeb.Data.Entities;
using System.Collections.Specialized;

namespace GestaoEscolarWeb.Models
{
    public class MyUserProfileViewModel 
    {

        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")]
        public string FullName { get; set; }


        public string UserName { get; set; }    


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


        public Guid? ImageId { get; set; }


        [Display(Name = "Profile Image")]
        public IFormFile ImageFile { get; set; }


        public string ImageFullPath => ImageId == null || ImageId == Guid.Empty
        ? $"/imagens/noImage.jpg"
        : $"https://gestaoescolar.blob.core.windows.net/imagens/{ImageId}";

    }
}
