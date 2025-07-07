using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Course : IEntity
    {
        public int Id { get; set; }


        [Required]
        public string Name { get; set; }

        
        [Display(Name ="Course Hours")]
        public int CourseHours => CourseSubjects == null? 0 : CourseSubjects.Sum(subject => subject.CreditHours);


        public ICollection<Subject> CourseSubjects { get; set; }

        public ICollection<SchoolClass> SchoolClasses { get; set; }
    }
}
