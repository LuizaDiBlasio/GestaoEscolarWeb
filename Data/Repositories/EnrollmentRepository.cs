using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Collections;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        private readonly DataContext _context;  
        public EnrollmentRepository(DataContext context) : base(context)
        {
            _context = context; 
        }

        public async Task<bool> ExistingEnrollmentAsync(Entities.Student student, CreateEnrollmentViewModel model)
        {
            return await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.SubjectId == model.SelectedSubjectId);
        }

        public async Task<IEnumerable> GetEnrollmentsWithStudentAndSubjectAsync()
        {
            return await _context.Set<Enrollment>()
                .Include (e => e.Student)   
                .Include (e => e.Subject)
                .ToListAsync(); 
        }

        
        public async Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id)
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
