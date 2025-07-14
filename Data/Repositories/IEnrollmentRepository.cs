using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndSubjectAsync();

        Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id);

        Task<bool> ExistingEnrollmentAsync(Student student, CreateEnrollmentViewModel model);

        Task<StudentStatus> GetStudentStatusAsync(Enrollment enrollment);

        Task<decimal> GetAverageScoreAsync(int enrollmentId);

        Task<List<ChartDataPoint>> GetStudentEnrollmentStatusPercentagesAsync(int studentId);

        Task<Enrollment> GetActiveEnrollment(int studentId, int subjectId);
    }
}
