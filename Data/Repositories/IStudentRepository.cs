using GestaoEscolarWeb.Data.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        public Task<IEnumerable<Student>> GetAllStudentsWithSchoolClass();

        public Task<Student> GetStudentWithSchoolClassEnrollmentsAndEvaluations(int id);
    }

}
