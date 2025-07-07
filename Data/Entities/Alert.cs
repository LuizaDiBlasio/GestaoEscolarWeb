using GestaoEscolarWeb.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Alert : IEntity
    {
        public int Id { get; set; }


        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AlertTime { get; set; }


        [Required]
        [StringLength(50, ErrorMessage = "The {0} field must be at most {1} characters long.")]
        [Display(Name = "Title")]
        public string MessageTitle { get; set; }


        [Required]
        [StringLength(400, ErrorMessage = "The {0} field must be at most {1} characters long.")]
        public string Message { get; set; }


        public string UserAuditId { get; set; }  


        public User UserAudit { get; set; }    


        public Status Status { get; set; }  

    }
}
