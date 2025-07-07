using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class CourseViewModel : Course
    {
        // Propriedade para guardar os IDs das Subjects selecionadas (por ser HTML, preciso usar os Ids e não os objetos)
        [Display(Name = "Subjects")]
        [MinElements(1, ErrorMessage = "Please select at least one subject.")] 
        public List<int> SelectedSubjectIds { get; set; }


        //Propriedade para mostrar todas as subject na checklist
        public List<SelectListItem> SubjectsToSelect { get; set; }
   
    }
}
