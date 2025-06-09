using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;

namespace GestaoEscolarWeb.Helpers
{
    public interface IConverterHelper
    {
        Course ToCourse(CourseViewModel model, bool isNew);
        CourseViewModel ToCourseViewModel(Course course);
    }
}
