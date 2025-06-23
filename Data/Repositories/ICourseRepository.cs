using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course> GetCourseSubjectsAndSchoolClassesByIdAsync(int id);

        IEnumerable<SelectListItem> GetComboCourses(); //preenche combobos de cursos na seleção de cursos da turma
    }
}
