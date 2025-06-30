using GestaoEscolarWeb.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Enrollment : IEntity
    {
        public int Id { get; set; }

        public Student? Student { get; set; }

        [Required]
        public int StudentId { get; set; }  

        public Subject? Subject { get; set; }

        [Required]
        public int SubjectId { get; set; }  


        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime EnrollmentDate { get; set; }


        [Display(Name = "Abscence Record")]
        public int AbscenceRecord { get; set; }


        [Display(Name = "Student Status")]
        public StudentStatus StudentStatus { get; set; } 

        public Enrollment() //atribuir status default como enrolled no ctor
        {
            StudentStatus = StudentStatus.Enrolled; 
        }


    }
}
