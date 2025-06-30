using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;
using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class CreateEditStudentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] //mensagem não chega a ser mostrada
        public string FullName { get; set; }


        [Required(ErrorMessage = "The field {0} is required.")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }


        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }


        [Required]
        public string Address { get; set; }


        [Required]
        public string Email { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "You need to select school class to enroll the student")]
        [Display(Name = "School class")]
        public int? SelectedSchoolClassId { get; set; }


        public IEnumerable<SelectListItem> AvailableSchoolClasses { get; set; }


        public Guid ProfileImageId { get; set; }


        [Display(Name = "Profile Image")]
        public IFormFile ImageFile { get; set; }


        public string ImageFullPath => ProfileImageId == Guid.Empty
              ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
    : $"https://gestaoescolar.blob.core.windows.net/imagens/{ProfileImageId}";
    }
}

