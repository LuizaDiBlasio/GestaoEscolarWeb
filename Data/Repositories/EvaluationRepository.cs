using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class EvaluationRepository : GenericRepository<Evaluation>, IEvaluationRepository
    {
        private readonly DataContext _context;
        public EvaluationRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        async Task<IEnumerable<Evaluation>> IEvaluationRepository.GetEvaluationsWithStudentsAndSubjectsAsync()
        {
            var evaluations = await _context.Evaluations
                                .Include(e => e.Student)
                                .Include(e => e.Subject)
                                .ToListAsync();
            return evaluations;
        }

        public async Task<bool> ExistingEvaluationAsync(CreateEditEvaluationViewModel model)
        {
            return await _context.Evaluations.AnyAsync(e => e.ExamDate == model.ExamDate && e.SubjectId == model.SelectedSubjectId);
        }

        public async Task<Evaluation> GetEvaluationWithStudentAndSubjectByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Evaluation>> GetStudentEvaluationsAsync(Student student)
        {
            return await _context.Evaluations
         .Include(e => e.Student)
         .Include(e => e.Subject)
         .Where(e => e.StudentId == student.Id)
         .ToListAsync();
        }
    }
}
