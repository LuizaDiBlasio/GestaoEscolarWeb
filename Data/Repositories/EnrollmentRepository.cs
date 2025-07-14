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


        /// <summary>
        /// Checks if an enrollment already exists for a specific student in a given subject.
        /// </summary>
        /// <param name="student">The "Student" entity.</param>
        /// <param name="model">The "CreateEnrollmentViewModel" containing the selected subject ID.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains "true" if an enrollment exists, otherwise "false".
        /// </returns>
        public async Task<bool> ExistingEnrollmentAsync(Entities.Student student, CreateEnrollmentViewModel model)
        {
            return await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.SubjectId == model.SelectedSubjectId);
        }


        /// <summary>
        /// Retrieves an active enrollment for a specific student in a given subject.
        /// An active enrollment is defined as one where the "Enrollment.StudentStatus" is "StudentStatus.Enrolled".
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <param name="subjectId">The ID of the subject.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains the "Enrollment" entity if found, otherwise "null".
        /// </returns>
        public async Task<Enrollment> GetActiveEnrollment(int studentId, int subjectId)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(
                      e => e.StudentId == studentId &&
                      e.SubjectId == subjectId &&
                      e.StudentStatus == StudentStatus.Enrolled);
        }


        /// <summary>
        /// Calculates the average score for a student in a specific subject based on their evaluations.
        /// </summary>
        /// <param name="enrollmentId">The ID of the enrollment.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains the average score. 
        /// Returns 0 if the enrollment or evaluations are not found, or if there are no evaluations for the subject.
        /// </returns>
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


        /// <summary>
        /// Retrieves all enrollments, including their associated student and subject entities.
        /// This method performs eager loading of related entities.
        /// </summary>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains an "IEnumerable{T}" of "Enrollment" entities,
        /// with the "Enrollment.Student" and "Enrollment.Subject" properties loaded.
        /// </returns>
        public async Task<IEnumerable<Enrollment>> GetEnrollmentsWithStudentAndSubjectAsync()
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .ToListAsync();
        }


        /// <summary>
        /// Retrieves a single enrollment by its ID, including its associated student and subject entities.
        /// This method performs eager loading of related entities.
        /// </summary>
        /// <param name="id">The ID of the enrollment to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that  contains the "Enrollment" entity if found,
        /// including its "Enrollment.Student" and "Enrollment.Subject" properties, otherwise "null".
        /// </returns>
        public async Task<Enrollment> GetEnrollmentWithStudentAndSubjectByIdAsync(int id)
        {
            return await _context.Set<Enrollment>()
                .Include(e => e.Student)
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        /// <summary>
        /// Calculates the percentage distribution of a student's enrollment statuses (Failed/Absent, Approved, Not Assessed)
        /// based on the credit hours of their subjects. This data is intended for use in charts.
        /// </summary>
        /// <param name="studentId">The ID of the student.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation that contains a "List{T}" of "ChartDataPoint" objects,
        /// each representing a category (Failed, Approved, Not Assessed) with its percentage and a recommended color.
        /// Returns an empty list if no enrollments are found for the student or if total credit hours are zero.
        /// </returns>
        public async Task<List<ChartDataPoint>> GetStudentEnrollmentStatusPercentagesAsync(int studentId)
        {
            var DataPointFailed = new ChartDataPoint();
            var DataPointApproved = new ChartDataPoint();
            var DataPointNotAssessed = new ChartDataPoint();
            var ChartDataPoints = new List<ChartDataPoint>(); 

            int totalCreditHours = 0;  
            int failedOrAbsentHours = 0;
            int approvedHours = 0;
            int notAssessedHours = 0;

            //buscar enrollments
            var enrollments = await _context.Enrollments
                .Include(e => e.Subject)
                .Where(e => e.StudentId == studentId)   
                .ToListAsync();

            if (!enrollments.Any())
            {
                return new List<ChartDataPoint>(); // caso não haja enrollemnts retornar lista vazia
            }

            foreach (Enrollment enrollment in enrollments)
            {
                totalCreditHours += enrollment.Subject.CreditHours;

                if(enrollment.StudentStatus == StudentStatus.Absent || enrollment.StudentStatus == StudentStatus.Failed)
                {
                    failedOrAbsentHours += enrollment.Subject.CreditHours;
                }

                if(enrollment.StudentStatus == StudentStatus.Approved)
                {
                    approvedHours += enrollment.Subject.CreditHours;
                }

                if(enrollment.StudentStatus == StudentStatus.Enrolled)
                {
                    notAssessedHours += enrollment.Subject.CreditHours;
                }

            }

            if (totalCreditHours == 0)
            {
                return new List<ChartDataPoint>(); // evitar divisão por 0
            }

            if(failedOrAbsentHours > 0) //se repetiu ou abandonou alguma materia
            {
                DataPointFailed.Percentage = ((decimal)failedOrAbsentHours / totalCreditHours) * 100M;
                DataPointFailed.Category = "Failed";
                DataPointFailed.Color = "#F44336"; //vermelho

                ChartDataPoints.Add(DataPointFailed);
            }
            
            if(approvedHours > 0)
            {
                DataPointApproved.Percentage = ((decimal)approvedHours / totalCreditHours) * 100M;
                DataPointApproved.Category = "Approved";
                DataPointNotAssessed.Color = "#4CAF50"; //verde 

                ChartDataPoints.Add(DataPointApproved);
            }
            
            if(notAssessedHours > 0)
            {
                DataPointNotAssessed.Percentage = ((decimal)notAssessedHours / totalCreditHours) * 100M;
                DataPointNotAssessed.Category = "Not Assessed";
                DataPointNotAssessed.Color = "#9E9E9E"; //cinza

                ChartDataPoints.Add(DataPointNotAssessed);
            }
     
            return ChartDataPoints; 
        }


        /// <summary>
        /// Determines the academic status of a student for a specific enrollment based on
        /// absence records and average evaluation scores, compared against system-defined limits.
        /// </summary>
        /// <param name="enrollmentSearch">An "Enrollment" entity to determine the status for. It should contain at least the Id.</param>
        /// <returns>
        /// A Task{TResult}" that represents the asynchronous operation that contains the determined "StudentStatus" (Enrolled, Approved, Failed, Absent, or Unknown).
        /// Returns StudentStatus.Unknown" if the enrollment, subject, or student data cannot be found.
        /// </returns>
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
