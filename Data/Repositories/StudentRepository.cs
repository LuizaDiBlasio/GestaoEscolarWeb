using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly DataContext _context;
        
        public StudentRepository(DataContext context) : base (context)
        {
            _context = context;
        }


        /// <summary>
        /// Retrieves all students, including their associated school class and the course of that school class.
        /// </summary>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation containing an IEnumerable{T}" of "Student" entities,
        /// with the "Student.SchoolClass" and its "SchoolClass.Course" properties loaded.
        /// </returns>
        public async Task<IEnumerable<Student>> GetAllStudentsWithSchoolClassAsync()
        {
            return await _context.Set<Student>()
                .Include(s => s.SchoolClass)
                    .ThenInclude(sc => sc.Course)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves a student entity by their email address.
        /// </summary>
        /// <param name="email">The email address of the student to retrieve.</param>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the "Student" entity if found, otherwise "null".
        /// </returns>
        public async Task<Student> GetStudentByEmailAsync(string email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email == email);  
        }


        /// <summary>
        /// Retrieves a collection of students by their full name.
        /// The search requires an exact match after trimming whitespace.
        /// Includes the associated school class for each student.
        /// </summary>
        /// <param name="studentFullName">The full name of the student to search for.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing an "IEnumerable{T}" of ="Student" entities
        /// matching the provided full name, with their "Student.SchoolClass" property loaded.
        /// Returns an empty list if "studentFullName" is null or empty, or if no students are found.
        /// </returns>
        public async Task<IEnumerable<Student>> GetStudentsByFullNameAsync(string studentFullName)
        {
            if (string.IsNullOrEmpty(studentFullName))
            {
                return new List<Student>(); // retorna
            }

            string cleanedFullName = studentFullName.Trim().ToLower();

            return await _context.Students
                .Where(s => s.FullName.ToLower() == cleanedFullName)
                .Include(s => s.SchoolClass)
                .ToListAsync(); 
        }


        /// <summary>
        /// Gets a list of "SelectListItem" representing the possible student statuses
        /// from the "StudentStatus" enumeration populate a dropdown list.
        /// </summary>
        /// <returns>
        /// A "List{T}" of "SelectListItem" where each item's Value and Text are the string representation of a "StudentStatus" enum.
        /// </returns>
        public List<SelectListItem> GetStudentStatusList()
        {
            return  Enum.GetValues(typeof(StudentStatus)) // indica o tipo do enum a buscar
                      .Cast<StudentStatus>() // converte a lista de ints do enum para uma IEnumerable<StudentStatus>
                      .Select(status => new SelectListItem // converte a lista anterior para uma SelectListItem
                      {
                          Value = status.ToString(), // "Enrolled", "Approved", "Failed"
                          Text = status.ToString()    // "Enrolled", "Approved", "Failed"
                      }).ToList();
        }


        /// <summary>
        /// Retrieves a student by their ID, including their associated enrollments and the subjects within those enrollments.
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation containing the "Student" entity if found,
        /// including its "Student.Enrollments" collection and the "Enrollment.Subject"
        /// property within each enrollment, otherwise "null">.
        /// </returns>
        public async Task<Student> GetStudentWithEnrollmentsAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        /// <summary>
        /// Retrieves a student by their ID, including their associated evaluations and the subjects for those evaluations.
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the "Student" entity if found, including its "Student.Evaluations" collection and the "Evaluation.Subject"
        /// property within each evaluation, otherwise "null">.
        /// </returns>
        public async Task<Student> GetStudentWithEvaluationsAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Evaluations)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        /// <summary>
        /// Retrieves a student by their ID, including their associated school class.
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing the "Student" entity if found,
        /// including its "Student.SchoolClass" property, otherwise "null".
        /// </returns>
        public async Task<Student> GetStudentWithSchoolClassAsync(int id)
        {
            return await _context.Students
                    .Include (s => s.SchoolClass)
                    .FirstOrDefaultAsync(s => s.Id == id);

        }


        /// <summary>
        /// Retrieves a student entity by their ID, including their school class,
        /// enrollments (with subjects), and evaluations (with subjects).
        /// </summary>
        /// <param name="id">The ID of the student to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation, containing:
        /// "Student" entity if found with its Student.SchoolClass" (including "SchoolClass.Course" and "Course.CourseSubjects"),
        /// "Student.Enrollments" (including "Enrollment.Subject")
        /// "Student.Evaluations" (including "Evaluation.Subject") properties loaded.
        /// Returns "null" if no student matches the ID.
        /// </returns>
        public async Task<Student> GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(int id)
        {
            return await _context.Set<Student>()
                .Include(s=> s.SchoolClass)
                    .ThenInclude(sc => sc.Course)
                        .ThenInclude(c => c.CourseSubjects)
                .Include(s=> s.Enrollments)
                    .ThenInclude(e => e.Subject)
                .Include(s=> s.Evaluations)
                    .ThenInclude (e => e.Subject)
                .FirstOrDefaultAsync(s => s.Id == id);  
        }


    }
}
