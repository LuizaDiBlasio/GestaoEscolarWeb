using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface ISchoolClassRepository : IGenericRepository<SchoolClass>
    {
        IEnumerable<SelectListItem> GetComboSchoolYears();

        IEnumerable<SelectListItem> GetComboShifts();

        Task<IEnumerable<SchoolClass>> GetAllSchoolClassesWithCourseAsync();

        Task<SchoolClass> GetSchoolClassCourseAndStudentsAsync(int id);

        Task<IEnumerable<SelectListItem>> GetComboSchoolClassesAsync();
    }
}
