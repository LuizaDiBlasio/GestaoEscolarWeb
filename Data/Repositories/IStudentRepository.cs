using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        public Task<IEnumerable<Student>> GetAllStudentsWithSchoolClassAsync();

        public Task<Student> GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(int id);

        public Task<Student> GetStudentByFullNameAsync(string studentFullName);

        public Task<Student> GetStudentWithEnrollmentsAsync(int id);

        public Task<Student> GetStudentWithEvaluationsAsync(int id);

        public List<SelectListItem> GetStudentStatusList();
    }

}
