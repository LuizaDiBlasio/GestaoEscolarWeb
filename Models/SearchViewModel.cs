using GestaoEscolarWeb.Data.Entities;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Models
{
    public class SearchViewModel<T>
    {
        [Required(ErrorMessage = "The field {0} is required.")]

        [Display (Name = "Insert student's full name")]
        public string StudentFullName { get; set; }


        public IEnumerable<T> Results { get; set; }


        public bool IsSearchSuccessful { get; set; } = false;


        public SearchViewModel() //para não ter que sempre instanciar a lista
        {
            Results = new List<T>(); 
        }
    }
}
