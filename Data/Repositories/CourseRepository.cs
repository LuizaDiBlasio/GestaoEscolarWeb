using GestaoEscolarWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Course> GetCourseWithSubjectsByIdAsync(int id) //o GetByIdAsync é lazyload, não carrega as listas
        {
            return await _context.Set<Course>() //ir para a tabela course
           .Include(c => c.CourseSubjects) // Isso carregará os Subjects relacionados
           //.ThenInclude(cs => cs.UserAudit) // quando precisar incluir UserAudit dos Subjects
           .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
