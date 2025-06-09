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

        
        [Required]
        [Display(Name ="Start Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate {  get; set; }


        [Required]
        [Display(Name ="End Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }    


        public User UserAudit { get; set; }


        public ICollection<Subject> CourseSubjects { get; set; }

        public ICollection<SchoolClass> SchoolClasses { get; set; }
    }
}
