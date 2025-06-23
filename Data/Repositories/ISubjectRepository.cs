using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        public Task<List<SelectListItem>> GetSubjectsToSelectAsync();

        public Task<Subject> GetSubjectWithCoursesAsync(int id);
    }
}
