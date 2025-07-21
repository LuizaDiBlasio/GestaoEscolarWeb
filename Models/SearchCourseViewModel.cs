using GestaoEscolarWeb.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;

namespace GestaoEscolarWeb.Models
{
    public class SearchCourseViewModel
    {
        [Required]
        [Display(Name="Insert Course Id:")]
        public int SearchId { get; set; }

        public int Id { get; set; }


        public string Name { get; set; }


        [Display(Name = "Course Hours")]
        public int CourseHours => CourseSubjects == null ? 0 : CourseSubjects.Sum(subject => subject.CreditHours);



        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }


        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }


        public ICollection<Subject> CourseSubjects { get; set; }

        public ICollection<SchoolClass> SchoolClasses { get; set; }

        public ICollection<Student> CourseStudents { get; set; }


        public bool IsSearchSuccessful { get; set; } = false;

        public SearchCourseViewModel()
        {
            CourseSubjects = new List<Subject>();

            SchoolClasses = new List<SchoolClass>();

            CourseStudents = new List<Student>();
        }


    }
}
