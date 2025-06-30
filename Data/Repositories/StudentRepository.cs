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

        public async Task<IEnumerable<Student>> GetAllStudentsWithSchoolClassAsync()
        {
            return await _context.Set<Student>()
                .Include(s => s.SchoolClass)
                .ToListAsync();
        }

        public async Task<Student> GetStudentByFullNameAsync(string studentFullName)
        {
            if (string.IsNullOrEmpty(studentFullName))
            {
                return null;
            }

            string cleanedFullName = studentFullName.Trim();
            return await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(s => s.FullName == studentFullName);   
        }

        public List<SelectListItem> GetStudentStatusList()
        {
            return  Enum.GetValues(typeof(StudentStatus))
                      .Cast<StudentStatus>()
                      .Select(status => new SelectListItem
                      {
                          Value = status.ToString(), // "Enrolled", "Approved", "Failed"
                          Text = status.ToString()    // "Enrolled", "Approved", "Failed"
                      }).ToList();
        }
        

        public async Task<Student> GetStudentWithEnrollmentsAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student> GetStudentWithEvaluationsAsync(int id)
        {
            return await _context.Students
                .Include(s => s.Evaluations)
                    .ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student> GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(int id)
        {
            return await _context.Set<Student>()
                .Include(s=> s.SchoolClass)
                    .ThenInclude(sc => sc.Course)
                .Include(s=> s.Enrollments)
                    .ThenInclude(e => e.Subject)
                .Include(s=> s.Evaluations)
                .FirstOrDefaultAsync(s => s.Id == id);  
        }


    }
}
