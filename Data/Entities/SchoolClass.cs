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
        public string Course { get; set; }

        [Required]
        public string Shift { get; set; }   

        public User UserAudit { get; set; }

        public ICollection<Student> Students { get; set; } 
    }
}
