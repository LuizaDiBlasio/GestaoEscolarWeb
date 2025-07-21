using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.ValidationAttributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class DashBoardViewModel
    {
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        [Display(Name = "Absence Limit (%)")]
        [Range(0, 100, ErrorMessage = "Please insert a valid number")]
        public decimal AbsenceLimitPercentage { get; set; }



        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        [Display(Name = "Passing Grade")]
        [Range(0, 20, ErrorMessage = "Please insert a valid number")]
        public decimal PassingGrade { get; set; }


        public IEnumerable<Alert> UserMessages { get; set; }

        public DashBoardViewModel()
        {
            UserMessages = new List<Alert>();
        }
    }
}
