using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;

namespace GestaoEscolarWeb.Controllers
{
    public class EvaluationsController : Controller
    {
        private readonly DataContext _context;

        private readonly IEvaluationRepository _evaluationRepository;

        private readonly IStudentRepository _studentRepository;

        private readonly IFlashMessage _flashMessage;  
        
        private readonly ISubjectRepository _subjectRepository;

        private readonly IConverterHelper _converterHelper;

        private readonly IUserHelper _userHelper;

        private readonly IEnrollmentRepository _enrollmentRepository;

        public EvaluationsController(IEvaluationRepository evaluationRepository, IStudentRepository studentRepository, 
            IFlashMessage flashMessage, ISubjectRepository subjectRepository, IConverterHelper converterHelper, IUserHelper userHelper,
            IEnrollmentRepository enrollmentRepository)
        {
            _evaluationRepository = evaluationRepository;

            _studentRepository = studentRepository;

            _flashMessage = flashMessage;   

            _subjectRepository = subjectRepository; 

            _converterHelper = converterHelper;

            _userHelper = userHelper;

            _enrollmentRepository = enrollmentRepository;
        }


        /// <summary>
        /// Displays a list of all evaluations, including associated students and subjects.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view displaying the list of evaluations.</returns>
        // GET: Evaluations
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
            return View(await _evaluationRepository.GetEvaluationsWithStudentsAndSubjectsAsync());
        }



        /// <summary>
        /// Displays the view for creating a new evaluation.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to create a new evaluation.</returns>
        // GET: Evaluations/Create
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult>  Create(int? id)
        {
            if (id != null)
            {
                var enrollment = await _enrollmentRepository.GetEnrollmentWithStudentAndSubjectByIdAsync(id.Value);

                if(enrollment == null)
                {
                    _flashMessage.Danger("Not possible to carry on with evaluation");
                    var modelEmpty = new CreateEditEvaluationViewModel();
                    return View(modelEmpty);
                }

                var student = await _studentRepository.GetStudentWithEnrollmentsAsync(enrollment.Student.Id);

                var subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);

                var modelExistingStudent = new CreateEditEvaluationViewModel()
                {
                    StudentId = enrollment.Student.Id,
                    StudentFullName = enrollment.Student.FullName,
                    Subjects = subjects,
                    SelectedSubjectId = enrollment.SubjectId

                };

                return View(modelExistingStudent);  
            }

            var model = new CreateEditEvaluationViewModel();

            return View(model);
        }


        /// <summary>
        /// Processes the creation of a new evaluation.
        /// Validates the student's enrollment, school year, exam date, and prevents duplicate evaluations.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the evaluation details.</param>
        /// <returns>Redirects to the Index view on successful creation, or returns the view with validation errors and flash messages.</returns>
        // POST: Evaluations/Create
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEditEvaluationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(model.StudentId);   //resolver esse metodo 

                if (student == null)
                {
                    _flashMessage.Danger("Not possible the complete evaluation, you need to enroll student first");
                    model.Subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);
                    return View(model);
                }

                //verificar se evaluation já existe pela chave composta
                var existingEvaluation = await _evaluationRepository.ExistingEvaluationAsync(model);

                if (existingEvaluation)
                {
                    _flashMessage.Warning("Evaluation is already registered.");
                    model.Subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);
                    return View(model);
                }

                //verificar datas válidas

                if (student.SchoolClass.SchoolYear < DateTime.Now.Year)
                {
                    _flashMessage.Danger("Cannot create evaluation after schoolyear end");
                    model.Subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);
                    return View(model);
                }

                var enrollment = await _enrollmentRepository.GetActiveEnrollment(model.StudentId, model.SelectedSubjectId);

                if (model.ExamDate < enrollment.EnrollmentDate)
                {
                    
                    _flashMessage.Danger($"The exam date cannot be before the enrollment date ({enrollment.EnrollmentDate:MM/dd/yyyy}) for this subject.");
                    model.Subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);
                    return View(model);
                }

                if(model.ExamDate > DateTime.Now)
                {
                    _flashMessage.Danger($"The exam date cannot be after today's date");
                    model.Subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);
                    return View(model);
                }

                //criar nova evaluation
                var evaluation = new Evaluation()
                {
                    StudentId = student.Id,
                    ExamDate = model.ExamDate.Value,
                    SubjectId = model.SelectedSubjectId,
                    Score = model.Score.Value,    
                };

                await _evaluationRepository.CreateAsync(evaluation);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        /// <summary>
        /// Displays the view for editing an existing evaluation.
        /// Retrieves the evaluation along with student and subject details.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the evaluation to edit.</param>
        /// <returns>A view with a form to edit the evaluation, pre-populated with existing data.</returns>
        // GET: Evaluations/Edit/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }
            
            var evaluation = await _evaluationRepository.GetEvaluationWithStudentAndSubjectByIdAsync(id.Value);

            if (evaluation == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            var student = await _studentRepository.GetStudentWithEnrollmentsAsync(evaluation.StudentId);

            if (student == null)
            {
                _flashMessage.Danger("Not possible to carry on with enrollment");

                var errorModel = _converterHelper.ToCreateEditEvaluationViewModel(evaluation, new List<SelectListItem>());
                return View(errorModel);
            }

            var subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);

            if (subjects == null)
            {
                _flashMessage.Danger("Not possible to carry on with enrollment");

                var errorModel = _converterHelper.ToCreateEditEvaluationViewModel(evaluation, new List<SelectListItem>());
                return View(errorModel);
            }

            var model = _converterHelper.ToCreateEditEvaluationViewModel(evaluation, subjects);

            return View(model);
        }


        /// <summary>
        /// Processes the update of an existing evaluation.
        /// Validates the school year and converts the view model to an entity before updating.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the evaluation being edited.</param>
        /// <param name="model">The view model containing the updated evaluation details.</param>
        /// <returns>Redirects to the Index view on successful update, or returns the view with validation errors.</returns>
        //POST: Evaluations/Edit/5
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEditEvaluationViewModel model)
        {

            if (id != model.Id)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            var student = await _studentRepository.GetStudentWithSchoolClassAsync(model.StudentId);

            var evaluation = _converterHelper.ToEvaluation(model, student, false);

            if (evaluation == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            if (student.SchoolClass.SchoolYear < DateTime.Now.Year)
            {
                _flashMessage.Danger("Cannot edit evaluation after schoolyear end");
                return View(model);
            }

            

            if (ModelState.IsValid)
            {
                try
                {
                    await _evaluationRepository.UpdateAsync(evaluation);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvaluationExists(evaluation.Id))
                    {
                        return new NotFoundViewResult("EvaluationNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        /// <summary>
        /// Displays the evaluation status for the currently logged-in student.
        /// Retrieves all enrollments and evaluations for the student, calculates their status and average scores.
        /// This action is accessible only by users with the 'Student' role.
        /// </summary>
        /// <returns>A view displaying the student's evaluation status and enrollment chart data.</returns>
        //GET 
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentEvaluationsStatus()
        {
            var username = this.User.Identity.Name; //pegar o username

            var user = await _userHelper.GetUserByEmailAsync(username);

            if (user == null)
            {
                _flashMessage.Danger("User not found");
                return View();
            }

            var student = await _studentRepository.GetStudentByEmailAsync(username);    

            if (student == null)
            {
                _flashMessage.Danger("Student not found");
                return View();
            }

            var studentEvaluationsAndEnrollments = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(student.Id);

            if (studentEvaluationsAndEnrollments == null)
            {
                _flashMessage.Danger("Student not found");
                return View();
            }

            //atribuir student status e average score
            foreach (Enrollment enrollment in studentEvaluationsAndEnrollments.Enrollments)
            {
                var studentStatus = await _enrollmentRepository.GetStudentStatusAsync(enrollment);
                enrollment.StudentStatus = studentStatus;

                var averageScore = await _enrollmentRepository.GetAverageScoreAsync(enrollment.Id);
                enrollment.AvarageScore = averageScore; 
            }
            
            var model = new StudentEvaluationsStatusViewModel()
            {
                Enrollments = student.Enrollments,
                Evaluations = student.Evaluations,
                StudentId = student.Id
            };

            model.EnrollmentStatusChartData = await _enrollmentRepository.GetStudentEnrollmentStatusPercentagesAsync(student.Id);

            return View(model);

        }


        /// <summary>
        /// Displays the confirmation view for deleting an evaluation.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the evaluation to delete.</param>
        /// <returns>A view confirming the deletion, or a "Evaluation Not Found" view if the evaluation does not exist.</returns>
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            var evaluation = await _evaluationRepository.GetByIdAsync(id.Value); 
            
            if (evaluation == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            return View(evaluation);
        }


        /// <summary>
        /// Confirms and processes the deletion of an evaluation.
        /// Prevents deletion if the school year has ended.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the evaluation to be deleted.</param>
        /// <returns>Redirects to the Index view on successful deletion, or returns an error view if deletion fails.</returns>
        // POST: Evaluations/Delete/5
        [Authorize(Roles = "Employee")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);

            if (evaluation == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            var student = await _studentRepository.GetStudentWithSchoolClassAsync(evaluation.StudentId);

            if (student.SchoolClass.SchoolYear < DateTime.Now.Year)
            {
                _flashMessage.Danger("Cannot delete evaluation after schoolyear end");
                return View();
            }

            try
            {
                await _evaluationRepository.DeleteAsync(evaluation);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                ViewBag.ErrorTitle = $"Failed to delete evaluation.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }

            
        }

        /// <summary>
        /// Retrieves a list of subjects available for evaluation for a specific student via an AJAX call.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the student.</param>
        /// <returns>A JSON array of <see cref="SelectListItem"/> representing the available subjects,
        /// or an error message if the student is not found or not enrolled in any subject.</returns>
        //Ajax
        [Authorize(Roles = "Employee")]
        [HttpGet]
        public async Task<IActionResult> GetSubjectsForStudentEvaluation(int id)
        {

            var student = await _studentRepository.GetStudentWithEnrollmentsAsync(id);

            if (student == null)
            {
                return NotFound("Student not found with the provided id.");
            }

            if (student.Enrollments == null)
            {
                return BadRequest("Not possible to register evaluation, student needs to be enrolled in a subject first.");
            }

            var subjects = await _subjectRepository.GetComboSubjectsToEvaluateAsync(student);

            // Se subjects for null ou vazia
            if (subjects == null || subjects.Count == 0)
            {
                return this.Json(new List<SelectListItem>()); // Retorna uma lista vazia
            }

            // Retorna a lista de SelectListItem como JSON
            return this.Json(subjects);
        }


        /// <summary>
        /// Checks if an evaluation with the specified ID exists in the database.
        /// </summary>
        /// <param name="id">The ID of the evaluation to check.</param>
        /// <returns><c>true</c> if an evaluation with the given ID exists; otherwise, <c>false</c>.</returns>
        private bool EvaluationExists(int id)
        {
            return _context.Evaluations.Any(e => e.Id == id);
        }


        /// <summary>
        /// Displays the "Evaluation Not Found" view when a requested evaluation cannot be located.
        /// </summary>
        /// <returns>The "Evaluation Not Found" view.</returns>
        public IActionResult EvaluationNotFound()
        {
            return View();
        }
    }
}
