using GestaoEscolarWeb.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;
using GestaoEscolarWeb.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class CreateEnrollmentViewModel
    {
        [Required]
        [Display(Name = "Student")]
        public string StudentFullName { get; set; }


        [Display(Name = "Subjects")]
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
