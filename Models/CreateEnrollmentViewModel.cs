using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GestaoEscolarWeb.Models
{
    public class CreateEnrollmentViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The student Id cannot be 0")]
        [Display(Name = "Student Id")]
        public int StudentId { get; set; }


        [Required]
        [Display(Name = "Full Name")]
        public string StudentFullName { get; set; }


        [Display(Name = "Subjects to enroll")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a subject.")]
        public int SelectedSubjectId { get; set; }


        public IEnumerable<SelectListItem> Subjects { get; set; }


        [Required]
        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EnrollmentDate { get; set; }

    }
}
