using GestaoEscolarWeb.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class SearchSubjectViewModel
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Subject name")]
        public string SearchSubjectName { get; set; }

        
        public string Name { get; set; }



        [Display(Name = "Credit Hours")]
        public int CreditHours { get; set; }



        public ICollection<Course> SubjectCourses { get; set; }


        public bool IsSearchSuccessful { get; set; }

        public SearchSubjectViewModel() 
        {
             SubjectCourses = new List<Course>();   
        }
    }
}
