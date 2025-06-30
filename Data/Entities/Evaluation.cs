using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Evaluation : IEntity
    {
        public int Id { get ; set ; }


        public Subject Subject { get ; set ; }

        public int SubjectId { get; set; }


        public Student Student { get ; set ; }

        public int StudentId { get; set; }


        [Display(Name = "Exam Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ExamDate { get ; set ; }


        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Score { get ; set ; }

    }
}
