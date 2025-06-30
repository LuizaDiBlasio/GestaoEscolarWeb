using GestaoEscolarWeb.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using GestaoEscolarWeb.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

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
         .Include(e => e.Student) // Já estava lá, mas não estritamente necessário se você já tem o student.Id
         .Include(e => e.Subject) // <-- Adicione isso para carregar o Subject
         .Where(e => e.StudentId == student.Id) // Filtra pelo Id do estudante na avaliação
         .ToListAsync();
        }
    }
}
