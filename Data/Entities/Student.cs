using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Student : IEntity
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Full Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] 
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }


        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }


        [Required]
        public string Address { get; set; }


        [Required]
        public string Email { get; set; }


        public string UserStudentId { get; set; }


        [Display(Name = "User Id")]
        public User UserStudent { get; set; }


        [Display(Name = "School Class")]
        public SchoolClass? SchoolClass { get; set; }


        public int? SchoolClassId { get; set; }


        [Display(Name = "Profile Image")]
        public Guid ProfileImageId { get; set; }


        public string ImageFullPath => ProfileImageId == Guid.Empty
             ? $"/imagens/noImage.jpg" // caminho relativo à raiz da aplicação!
   : $"https://gestaoescolar.blob.core.windows.net/imagens/{ProfileImageId}";


        public ICollection<Evaluation> Evaluations { get; set; }


        public ICollection<Enrollment> Enrollments { get; set; }


    }
}
