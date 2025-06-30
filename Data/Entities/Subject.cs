using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Subject : IEntity
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(50, ErrorMessage = "The field {0} allows only {1} characters")] //mensagem não chega a ser mostrada
        public string Name { get; set; }


        [Required]
        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }


        public ICollection<Course> SubjectCourses { get; set; }

    }
}
