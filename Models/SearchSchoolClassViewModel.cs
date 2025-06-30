using GestaoEscolarWeb.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class SearchSchoolClassViewModel 
    {
        [Required(ErrorMessage = "The field {0} is required.")]
        [Display (Name ="Insert school class Id ")]
        public int SearchId { get; set; }


        [Display(Name = "School Year")]
        public int SchoolYear { get; set; }


        public Course Course { get; set; }


        public string Shift { get; set; }


        public ICollection<Student> Students { get; set; }

        public ICollection<Subject> CourseSubjects { get; set; }

        public bool IsSearchSuccessful { get; set; } = false;

        public SearchSchoolClassViewModel()
        {
            Students = new List<Student>(); 

            CourseSubjects = new List<Subject>();   
        }
    }
}
