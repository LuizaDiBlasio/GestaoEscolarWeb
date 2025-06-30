using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class EditEnrollmentViewModel
    {
        public int Id { get; set; } 


        [Display(Name = "Student")]
        public string StudentFullName { get; set; }


        [Display(Name = "Subject")]
        [Required]
        public int SelectedSubjectId { get; set; }

        public IEnumerable<SelectListItem> Subjects { get; set; }


        [Display(Name = "Enrollment Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Required]
        public DateTime EnrollmentDate { get; set; }


        [Display(Name = "Abscence Record")]
        public int AbscenceRecord { get; set; }


        [Display(Name = "Student Status")]
        public StudentStatus StudentStatus { get; set; }


        public IEnumerable<SelectListItem> StudentStatusList { get; set; }
    }
}
