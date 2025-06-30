using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        private readonly DataContext _context;

        public CourseRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetComboCourses()
        {
            var list = _context.Courses.Select(c => new SelectListItem 
            {
                //preencher as propriedades da combobox
                Text = c.Name,
                Value = c.Id.ToString()
            }).OrderBy(i => i.Text).ToList(); //ordenar por nome

            //primeiro item fora do range para colocar um placeholder
            list.Insert(0, new SelectListItem
            {
                Text = "Select a course...",
                Value = "0"
            });

            return list; //vai direto pro html na combo
        }

        public async Task<Course> GetCourseSubjectsAndSchoolClassesByIdAsync(int id) //o GetByIdAsync é lazyload, não carrega as listas
        {
            return await _context.Set<Course>() //ir para a tabela course
           .Include(c => c.CourseSubjects) // Isso carregará os Subjects relacionados
           .Include(sc => sc.SchoolClasses)
           .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Student>> GetStudentsFromCourseAsync(int courseId)
        {
           return await _context.Courses
                    .Where(c => c.Id == courseId)
                    .SelectMany(c => c.SchoolClasses.SelectMany(sc => sc.Students)) // Acha os alunos de todas as SchoolClasses do curso
                    .ToListAsync();
        }
    }
}
