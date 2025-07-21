using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestaoEscolarWeb.Models
{
    public class ManageTicketViewModel
    {
        public int Id { get; set; }    


        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AlertTime { get; set; }


       
        [Display(Name = "Title")]
        public string MessageTitle { get; set; }


        [Required]
        public string Message { get; set; }


        [Display(Name = "User's full name")]
        public string UserFullName { get; set; }



        [Display(Name = "Status")]
        public Status SelectedStatus { get; set; }


        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
