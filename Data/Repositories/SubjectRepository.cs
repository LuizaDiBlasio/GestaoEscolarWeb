using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
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


        /// <summary>
        /// Retrieves a list of all subjects formatted for use in a selectable list .
        /// </summary>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation containing a "List{T}" of "SelectListItem",
        /// where each item represents a subject with its ID as Value and Name as Text,
        /// ordered alphabetically by name.
        /// </returns>
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


        /// <summary>
        /// Retrieves a list of subjects available for a student to enroll in.
        /// These subjects are determined by the course associated with the student's school class.
        /// </summary>
        /// <param name="student">The "Student" entity whose associated school class and course will be used to filter subjects.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing a "List{T}" of "SelectListItem",
        /// where each item represents an enrollable subject with its ID as Value and Name as Text,
        /// ordered alphabetically by name. Returns "null" if the student's school class is not set.
        /// </returns>
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


        /// <summary>
        /// Retrieves a subject by its ID, including its associated courses through the linking entity.
        /// </summary>
        /// <param name="id">The ID of the subject to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing the "Subject" entity if found,
        /// including its "Subject.SubjectCourses" collection, otherwise "null".
        /// </returns>
        public async Task<Subject> GetSubjectWithCoursesAsync(int id)
        {
            var subject = await _context.Subjects
                               .Include(s => s.SubjectCourses)
                               .FirstOrDefaultAsync(s => s.Id == id);    

            return subject; 
        }


        /// <summary>
        /// Retrieves a list of subjects that a student is currently enrolled in.
        /// </summary>
        /// <param name="studentWithEnrollments">The "Student" entity with its "Student.Enrollments" loaded.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing a "List{T}" of "SelectListItem",
        /// where each item represents an enrolled subject with its ID as Value and Name as Text,
        /// ordered alphabetically by name.
        /// </returns>
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


        /// <summary>
        /// Retrieves a subject by its name.
        /// The search requires an exact match after trimming whitespace.
        /// Includes the associated courses for the subject.
        /// </summary>
        /// <param name="subjectName">The name of the subject to search for.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing the "Subject" entity if found,
        /// including its "Subject.SubjectCourses" collection, otherwise "null".
        /// Returns "null" if "subjectName" is null or empty.
        /// </returns>
        public async Task<Subject> GetSubjectByNameAsync(string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
            {
                return null;
            }

            string cleanedName = subjectName.Trim().ToLower();
            return await _context.Subjects
                .Include(s => s.SubjectCourses)
                .FirstOrDefaultAsync(s => s.Name.ToLower() == cleanedName);
        }


        /// <summary>
        /// Checks if a subject with the same name and credit hours already exists.
        /// </summary>
        /// <param name="subject">The "Subject" entity to check for existence.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing "true" if a subject with matching name and credit hours exists, otherwise "false".
        /// </returns>
        public async Task<bool> ExistingSubject(Subject subject)
        {
            return await _context.Subjects.AnyAsync(s => s.CreditHours == subject.CreditHours && s.Name == subject.Name);
        }

    }
}
