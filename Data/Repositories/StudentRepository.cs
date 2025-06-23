using GestaoEscolarWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Collections.Generic;
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

        public async Task<IEnumerable<Student>> GetAllStudentsWithSchoolClass()
        {
            return await _context.Set<Student>()
                .Include(s => s.SchoolClass)
                .ToListAsync();
        }

        public async Task<Student> GetStudentWithSchoolClassEnrollmentsAndEvaluations(int id)
        {
            return await _context.Set<Student>()
                .Include(s=> s.SchoolClass)
                    .ThenInclude(sc => sc.Course)
                .Include(s=> s.Enrollments)
                .Include(s=> s.Evaluations)
                .FirstOrDefaultAsync(s => s.Id == id);  
        }
    }
}
