using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        private readonly DataContext _context;

        public SubjectRepository(DataContext context) : base (context)
        {
            _context = context;
        }
     

        public async Task<List<SelectListItem>> GetSubjectsToSelectAsync()
        {
            //Popular a checklist com disciplinas disponíveis para escolha
            var subjectsToSelect = await _context.Subjects
                             .Select(s => new SelectListItem //compor lista
                             {
                                 Value = s.Id.ToString(),
                                 Text = s.Name
                             })
                             .OrderBy(s => s.Text) // Ordenar lista
                             .ToListAsync();

            return subjectsToSelect;    
        }

        public async Task<List<SelectListItem>> GetComboSubjectsToEnrollAsync(Student student)
        {
            //Popular a checklist com disciplinas que pertencem ao curso da turma do aluno
            if(student.SchoolClass != null)
            {
                var subjects = await _context.Students.Where(s => s.Id == student.Id)
                             .Include(s => s.SchoolClass)
                             .ThenInclude(sc => sc.Course)
                             .ThenInclude(c => c.CourseSubjects)
                             .SelectMany(s => s.SchoolClass.Course.CourseSubjects)
                             .Select(s => new SelectListItem //compor lista
                             {
                                 Value = s.Id.ToString(),
                                 Text = s.Name
                             })
                             .OrderBy(s => s.Text) // Ordenar lista
                             .ToListAsync();

                return subjects;
            }

            return null; 
        }

        public async Task<Subject> GetSubjectWithCoursesAsync(int id)
        {
            var subject = await _context.Subjects
                               .Include(s => s.SubjectCourses)
                               .FirstOrDefaultAsync(s => s.Id == id);    

            return subject; 
        }

        public async Task<List<SelectListItem>> GetComboSubjectsToEvaluateAsync(Student studentWithEnrollments)
        {
            var subjects = await _context.Enrollments
                                .Where(e => e.Student.Id == studentWithEnrollments.Id)   
                                .Select(e => e.Subject)
                                .Select(s => new SelectListItem
                                {
                                    Value = s.Id.ToString(),
                                    Text = s.Name
                                })
                                 .OrderBy(s => s.Text) // Ordenar lista
                                 .ToListAsync();
            return subjects;

        }

        public async Task<Subject> GetSubjectByNameAsync(string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return null;
            }

            string cleanedName = subjectName.Trim();
            return await _context.Subjects
                .Include(s => s.SubjectCourses)
                .FirstOrDefaultAsync(s => s.Name == cleanedName);
        }
    }
}
