using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Enrollment : IEntity
    {
        public int Id { get; set; }


        [Required]
        public Student Student { get; set; }


        [Required]
        public Subject Subject { get; set; }


        [Display(Name = "Enrollment Date")]
        [Required]
        public DateTime EnrollmentDate { get; set; }


        [Display(Name = "Abscence Record")]
        public int AbscenceRecord { get; set; }


        [Display(Name = "Student Status")]
        public int StudentStatus { get; set; }  


        public User UserAudit { get; set; }
    }
}
