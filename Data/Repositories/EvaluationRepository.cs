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


        /// <summary>
        /// Retrieves all evaluations, including their associated student and subject entities.
        /// </summary>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing an "IEnumerable{T}" of "Evaluation" entities,
        /// with the "Evaluation.Student" and "Evaluation.Subject" properties loaded.
        /// </returns>
        async Task<IEnumerable<Evaluation>> IEvaluationRepository.GetEvaluationsWithStudentsAndSubjectsAsync()
        {
            var evaluations = await _context.Evaluations
                                .Include(e => e.Student)
                                .Include(e => e.Subject)
                                .ToListAsync();
            return evaluations;
        }


        /// <summary>
        /// Checks if an evaluation with the same exam date and subject already exists.
        /// </summary>
        /// <param name="model">The "CreateEditEvaluationViewModel" containing the evaluation details to check.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing "true" if an existing evaluation matches the criteria, otherwise "false".
        /// </returns>
        public async Task<bool> ExistingEvaluationAsync(CreateEditEvaluationViewModel model)
        {
            return await _context.Evaluations.AnyAsync(e => e.ExamDate == model.ExamDate && e.SubjectId == model.SelectedSubjectId);
        }


        /// <summary>
        /// Retrieves a single evaluation by its ID, including its associated student and subject entities.
        /// </summary>
        /// <param name="id">The ID of the evaluation to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing the "Evaluation" entity if found,
        /// with its "Evaluation.Student" and "Evaluation.Subject" properties, otherwise "null".
        /// </returns>
        public async Task<Evaluation> GetEvaluationWithStudentAndSubjectByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        /// <summary>
        /// Retrieves all evaluations for a specific student, including their associated student and subject entities.
        /// This method performs eager loading of related entities.
        /// </summary>
        /// <param name="student">The "Student" entity for whom to retrieve evaluations.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containg an "IEnumerable{T}" of "Evaluation" entities
        /// belonging to the specified student, with their "Evaluation.Student" and "Evaluation.Subject" properties loaded.
        /// </returns>
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
