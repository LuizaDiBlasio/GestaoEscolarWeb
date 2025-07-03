using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Subject : IEntity
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] 
        public string Name { get; set; }


        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Credit Hours must be greater than zero.")]
        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }


        public ICollection<Course> SubjectCourses { get; set; }

    }
}
