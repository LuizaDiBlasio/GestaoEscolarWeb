using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Student : IEntity
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] //mensagem não chega a ser mostrada
        public string FirstName { get; set; }


        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] //mensagem não chega a ser mostrada
        public string LastName { get; set; }


        [Required]
        [Display(Name = "Date of Birth")]
        public DateTime BirthDate { get; set; }


        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }


        [Required]
        public string Address { get; set; }


        [Required]
        public string Email { get; set; }


        [Display(Name = "User Id")]
        public User UserStudent { get; set; }


        public User UserAudit { get; set; }


        [Required]
        [Display(Name = "School Class")]
        public SchoolClass SchoolClass { get; set; }


        [Required]
        [Display(Name = "Profile Image")]
        public Guid ProfileImageId { get; set; }


        public ICollection<Evaluation> Evaluations { get; set; }


        public ICollection<Enrollment> Enrollments { get; set; }


        //Propriedade de leitura 
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

    }
}
