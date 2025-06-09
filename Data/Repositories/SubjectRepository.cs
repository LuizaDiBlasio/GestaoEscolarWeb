using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        private readonly DataContext _context;

        public SubjectRepository(DataContext context) : base (context)
        {
            _context = context;
        }
     

        public async Task<List<SelectListItem>> GetSubjectsToSelectAsync()
        {
            //Popular a checklist com disciplinas disponíveis para escolha
            var subjectsToSelect = await _context.Subjects
                             .Select(s => new SelectListItem //compor lista
                             {
                                 Value = s.Id.ToString(),
                                 Text = s.Name
                             })
                             .OrderBy(s => s.Text) // Ordenar lista
                             .ToListAsync();

            return subjectsToSelect;    
        }

    }
}
