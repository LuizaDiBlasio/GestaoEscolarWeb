using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using GestaoEscolarWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        private readonly DataContext _context;

        private readonly IStudentRepository _studentRepository;

        private readonly ISystemDataService _systemDataService;

        private readonly IEvaluationRepository _evaluationRepository;

        public EnrollmentRepository(DataContext context, IStudentRepository studentRepository, ISystemDataService systemDataService, IEvaluationRepository evaluationRepository) : base(context)
        {
            _context = context;
            _studentRepository = studentRepository;
            _systemDataService = systemDataService;
            _evaluationRepository = evaluationRepository;   
        }

        public async Task<bool> ExistingEnrollmentAsync(Entities.Student student, CreateEnrollmentViewModel model)
        {
            return await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.SubjectId == model.SelectedSubjectId);
        }

        public async Task<decimal> GetAverageScoreAsync(int enrollmentId)
        {
            var enrollment = await GetEnrollmentWithStudentAndSubjectByIdAsync(enrollmentId);

            if (enrollment == null)
            {
                return 0;    
            }

            var evaluations = await _evaluationRepository.GetStudentEvaluationsAsync(enrollment.Student);

            if (evaluations == null)
            {
                return 0;    
            }

            decimal totalScore = 0;
            int count = 0;

            var evaluationsFromSubject = evaluations.Where(e => e.SubjectId == enrollment.SubjectId).ToList(); 

            if (evaluationsFromSubject.Any())
            {
                foreach (Evaluation evaluation in evaluationsFromSubject)
                {
                    totalScore += evaluation.Score;

                    count++;
                }
            }

            if(count > 0)
            {
                decimal average = totalScore / count;

                return average;
            }

            return 0;     
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndSubjectAsync()
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .ToListAsync();
        }


        public async Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id)
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<StudentStatus> GetStudentStatusAsync(Enrollment enrollmentSearch)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == enrollmentSearch.Id);  

            var systemData = await _systemDataService.GetSystemDataAsync();

            if (enrollment == null || enrollment.Subject == null || enrollment.Student == null )
            {
                return StudentStatus.Unknown;
            }

            if(enrollment.Subject.CreditHours > 0)
            {
                if (enrollment.AbscenceRecord / (decimal)enrollment.Subject.CreditHours > systemData.AbsenceLimit)
                {
                    return StudentStatus.Absent | StudentStatus.Failed;
                }
            }            

            var student = await _studentRepository.GetStudentWithEvaluationsAsync(enrollment.Student.Id);

            if (student == null)
            {
                return StudentStatus.Unknown;
            }

            decimal totalScore = 0;
            int count = 0;

            if (student.Evaluations.Count > 0)
            {
                foreach (Evaluation evaluation in student.Evaluations)
                {
                    if (evaluation.Subject.Id == enrollment.Subject.Id)
                    {
                        totalScore += evaluation.Score;

                        count++;
                    }
                }

                if (count > 0)
                {
                    if (totalScore / count < systemData.PassingGrade)
                    {
                        return StudentStatus.Failed;
                    }

                    return StudentStatus.Approved;
                }

                return StudentStatus.Enrolled; //não possui avaliações nessa materia mas está inscrito

            }

            return StudentStatus.Enrolled;
        }

    }
}
