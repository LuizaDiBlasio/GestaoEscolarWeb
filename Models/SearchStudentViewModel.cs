using GestaoEscolarWeb.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using GestaoEscolarWeb.Data.Entities.Enums;
using Microsoft.AspNetCore.Http;

namespace GestaoEscolarWeb.Models
{
    public class SearchStudentViewModel
    {
       
        public int Id { get; set; }


        [Display(Name = "Insert student's full name")]
        [Required]
        public string SearchFullName { get; set; }


        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }


        public string PhoneNumber { get; set; }


        public string Address { get; set; }


        [Required]
        public string Email { get; set; }


        public string UserStudentId { get; set; }


        public string SchoolClass { get; set; }

       
        public Guid ProfileImageId { get; set; }


        public string ImageFullPath => ProfileImageId == Guid.Empty
             ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
   : $"https://gestaoescolar.blob.core.windows.net/imagens/{ProfileImageId}";

        public bool HasHomonyms { get; set; } = false;


        public List<Student> HomonymStudents { get; set; }


        public bool IsSearchSuccessful { get; set; } = false;

        public SearchStudentViewModel()
        {
            HomonymStudents = new List<Student>();  
        }


    }
}
