using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System;
using GestaoEscolarWeb.Data.Entities;

namespace GestaoEscolarWeb.Models
{
    public class MyUserProfileViewModel : User
    {

        [Display(Name = "Profile Image")]
        public IFormFile ImageFile { get; set; }

    }
}
