using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace GestaoEscolarWeb.Helpers
{
    public class ConverterHelper : IConverterHelper
    {

        //TODO ainda não usei ToCourse, ver se é completamente necessário
        public Course ToCourse(CourseViewModel model, bool isNew)
        {
            return new Course
            {
                Id = isNew ? 0 : model.Id,
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
         }

        public CourseViewModel ToCourseViewModel(Course course)
        {
            return new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                CourseSubjects = course.CourseSubjects,
                SelectedSubjectIds = course.CourseSubjects? //evitar erro caso CourseSubjects seja null
                                   .Select(s => s.Id) //selecionar por id
                                   .ToList() ?? new List<int>() // se a lista criada pela busca for nula, criar uma nova lista

            };

        }
    }
}
