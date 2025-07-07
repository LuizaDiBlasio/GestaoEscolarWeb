using GestaoEscolarWeb.Data.Entities;
using System.Collections;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class StudentEvaluationsStatusViewModel
    {
       public int StudentId { get; set; }   

       public IEnumerable<Evaluation> Evaluations { get; set; }

       public IEnumerable<Enrollment> Enrollments { get; set; }

        public StudentEvaluationsStatusViewModel()
        {
             Evaluations = new List<Evaluation>();  

            Enrollments = new List<Enrollment>();   
        }

    }
}
