using GestaoEscolarWeb.Data.Entities;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course> GetCourseWithSubjectsByIdAsync(int id); 
    }
}
