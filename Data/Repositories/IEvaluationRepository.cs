using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IEvaluationRepository : IGenericRepository<Evaluation>
    {
        Task<IEnumerable<Evaluation>> GetEvaluationsWithStudentsAndSubjectsAsync();

        Task<bool> ExistingEvaluationAsync(CreateEditEvaluationViewModel model);

        Task<Evaluation> GetEvaluationWithStudentAndSubjectByIdAsync(int id);

        Task<IEnumerable<Evaluation>> GetStudentEvaluationsAsync(Entities.Student student);
    }
}
