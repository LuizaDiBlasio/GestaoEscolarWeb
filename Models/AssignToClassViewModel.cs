using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class AssignToClassViewModel
    {
        [Required]
        [Display(Name ="Student Id")]
        public int? StudentId { get; set; }


        [Display(Name ="Full Name")]
        public string StudentFullName { get; set; }


        [Required]
        [Display(Name = "School classes available")]
        [Range(1, int.MaxValue, ErrorMessage = "You need to select school class to enroll the student")]
        public int? SelectedSchoolClassId { get; set; }  

        public IEnumerable<SelectListItem>  AvailableSchoolClasses { get; set; }

        public AssignToClassViewModel()
        {
            AvailableSchoolClasses = new List<SelectListItem>();
        }
    }
}
