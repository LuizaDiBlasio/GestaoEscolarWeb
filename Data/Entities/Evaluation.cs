using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Evaluation : IEntity
    {
        public int Id { get ; set ; }


        public Subject Subject { get ; set ; }


        public Student Student { get ; set ; }


        [Display(Name = "Exam Date")]
        public DateTime ExamDate { get ; set ; }


        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public double Score { get ; set ; }

        //TODO ver se isso vai ser necessário
        public User UserAudit {  get ; set ; } 
    }
}
