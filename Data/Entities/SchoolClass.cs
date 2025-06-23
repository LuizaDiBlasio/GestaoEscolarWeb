using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class SchoolClass : IEntity 
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "School Year")]
        public int SchoolYear { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public Course Course { get; set; }


        [Required]
        public string Shift { get; set; }


        //TODO ver se isso vai ser necessário
        public User UserAudit { get; set; }

        public string CourseYearShift => $"{Course.Name} - {SchoolYear} - {Shift}"; //propriedade ara a view Details

        public override string ToString()
        {
            return $"{Course.Name} - {SchoolYear} - {Shift}";   //override para a lista AvailableSchooClasses (combobox)
        }

        public ICollection<Student> Students { get; set; } 
    }
}
