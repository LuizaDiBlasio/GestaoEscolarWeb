using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class CreateEditSchoolClassViewModel 
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Course")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a course.")]
        public int SelectedCourseId { get; set; }


        public IEnumerable<SelectListItem> AvailableCourses { get; set; }

        [Required]
        [Display(Name = "Year")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a year.")]
        public int SelectedYear { get; set; }


        public IEnumerable<SelectListItem> SchoolYears { get; set; }


        [Required]
        [Display(Name = "Shift")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a shift.")]
        public int SelectedShiftId { get; set; }


        public IEnumerable<SelectListItem> Shifts { get; set; }
    }
}
