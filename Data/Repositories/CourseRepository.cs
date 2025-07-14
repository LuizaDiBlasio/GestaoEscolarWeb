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


        /// <summary>
        /// Retrieves a list of courses formatted for use in a dropdown (combo box).
        /// Includes a placeholder item at the beginning of the list.
        /// </summary>
        /// <returns>
        /// An "IEnumerable{T}" of type "SelectListItem",
        /// where each item represents a course with its Name as Text and Id as Value,
        /// ordered alphabetically by name, and including a "Select a course..." placeholder.
        /// </returns>
        public IEnumerable<SelectListItem> GetComboCourses()
        {
            var list = _context.Courses
                .Select(c => new SelectListItem 
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


        /// <summary>
        /// Retrieves a single course by its ID, including its associated subjects and school classes.
        /// This method performs eager loading of related entities.
        /// </summary>
        /// <param name="id">The ID of the course to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains the "Course" entity if found,
        /// including its "Course.CourseSubjects" and "Course.SchoolClasses" collections,
        /// otherwise  it returns "null">.
        /// </returns>
        public async Task<Course> GetCourseSubjectsAndSchoolClassesByIdAsync(int id) //o GetByIdAsync é lazyload, não carrega as listas
        {
            return await _context.Set<Course>() //ir para a tabela course
           .Include(c => c.CourseSubjects) // Isso carregará os Subjects relacionados
           .Include(sc => sc.SchoolClasses)
           .FirstOrDefaultAsync(e => e.Id == id);
        }


        /// <summary>
        /// Retrieves all students associated with a specific course.
        /// This method navigates through school classes linked to the course to find all students.
        /// </summary>
        /// <param name="courseId">The ID of the course.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains an "IEnumerable{T}" of "Student" entities
        /// who are enrolled in school classes belonging to the specified course.</returns>
        public async Task<IEnumerable<Student>> GetStudentsFromCourseAsync(int courseId)
        {
           return await _context.Courses
                    .Where(c => c.Id == courseId)
                    .SelectMany(c => c.SchoolClasses.SelectMany(sc => sc.Students)) // Acha os alunos de todas as SchoolClasses do curso
                    .ToListAsync();
        }
    }
}
