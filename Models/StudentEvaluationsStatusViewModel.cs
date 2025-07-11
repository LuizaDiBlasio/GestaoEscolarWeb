using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using Syncfusion.EJ2.PivotView;
using System.Collections;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class StudentEvaluationsStatusViewModel
    {
       public int StudentId { get; set; }

       public List<ChartDataPoint> EnrollmentStatusChartData { get; set; }

       public IEnumerable<Evaluation> Evaluations { get; set; }

       public IEnumerable<Enrollment> Enrollments { get; set; }

        public StudentEvaluationsStatusViewModel()
        {
             Evaluations = new List<Evaluation>();  

            Enrollments = new List<Enrollment>();   
        }

    }
}
