using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class CreateEditEvaluationViewModel
    {
        public int Id { get; set; } 


        [Range(1, int.MaxValue, ErrorMessage = "The student Id cannot be 0")]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public string StudentFullName { get; set; }


        [Required]
        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExamDate { get; set; }


        [Required]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal? Score { get; set; }


        [Required]
        [Display(Name = "Subject")]
        public int SelectedSubjectId { get; set; }


        public IEnumerable<SelectListItem> Subjects { get; set; }

        public CreateEditEvaluationViewModel()
        {
            Subjects = new List<SelectListItem>();
        }

    }
}
