using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using System.Collections;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<IEnumerable> GetEnrollmentsWithStudentAndSubjectAsync();

        Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id);

         Task<bool> ExistingEnrollmentAsync(Student student, CreateEnrollmentViewModel model);
    }
}
