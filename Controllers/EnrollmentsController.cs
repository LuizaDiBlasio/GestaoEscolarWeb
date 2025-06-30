using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;


namespace GestaoEscolarWeb.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly DataContext _context;

        private readonly IEnrollmentRepository _enrollmentRepository; 
        
        private readonly IStudentRepository _studentRepository;

        private readonly ISubjectRepository _subjectRepository;

        private readonly IFlashMessage _flashMessage;

        private readonly IConverterHelper _converterHelper;

        public EnrollmentsController(DataContext context, IEnrollmentRepository enrollmentRepository, IStudentRepository studentRepository, 
            ISubjectRepository subjectRepository, IFlashMessage flashMessage, IConverterHelper converterHelper)
        {
            _context = context;

            _enrollmentRepository = enrollmentRepository;

            _studentRepository = studentRepository;

            _subjectRepository = subjectRepository;

            _flashMessage = flashMessage;   

            _converterHelper = converterHelper;
        }

        // GET: Enrollments
        public async Task<IActionResult> Index()
        {
            return View(await _enrollmentRepository.GetEnrollmentsWithStudentAndSubjectAsync());
        }

        // GET: Enrollments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            var enrollment = await _enrollmentRepository.GetEnrollmentWithStudentAndSubjectByIdAsync(id.Value);
            if (enrollment == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            return View(enrollment);
        }

        // GET: Enrollments/Create
        public async Task<IActionResult> Create(int? studentId)
        {
            CreateEnrollmentViewModel model;

            if (studentId != null)
            {
                var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(studentId.Value);

                if (student == null)
                {
                    _flashMessage.Danger("Student not found.");
                    return RedirectToAction(nameof(Create)); 
                }

                // verificar se aluno tem turma
                if (student.SchoolClass == null)
                {
                    _flashMessage.Warning("Enrollment failed, student needs to be enrolled in a school class first. Got to Edit Student section to enroll student in a school class");
                    return RedirectToAction(nameof(Create));
                }

                var subjects = await _subjectRepository.GetComboSubjectsToEnrollAsync(student);

                // Verifica se a lista de subjects é null ou vazia
                if (subjects == null || subjects.Count <= 1) // Count <= 1 por causa do "Select a subject..." 
                {
                    _flashMessage.Warning("Student's school class course has no subjects defined. Please ensure subjects are associated with the course.");

                    model = new CreateEnrollmentViewModel()
                    {
                        StudentFullName = student.FullName                    
                    };

                    return RedirectToAction(nameof(Create), model);
                }

                model = new CreateEnrollmentViewModel()
                {
                    StudentFullName = student.FullName,
                    Subjects = subjects
                };
            }
            else
            {
                // Caso studentId seja null
                //  disciplinas serão populadas via AJAX
                model = new CreateEnrollmentViewModel()
                {
                    Subjects = new List<SelectListItem>() // Lista inicial vazia para o dropdown
                };
            }

            return View(model);  
        }

        // POST: Enrollments/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateEnrollmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // checar nome 
                var student = await _studentRepository.GetStudentByFullNameAsync(model.StudentFullName);   //resolver esse metodo 

                if (student == null)
                {
                    _flashMessage.Danger("Not possible the complete enrollment, you need to register student first");
                    return RedirectToAction(nameof(Create), model);
                }

                //verificar se enrollment já existe pela chave composta
                var existingEnrollment = await _enrollmentRepository.ExistingEnrollmentAsync(student, model);

                if (existingEnrollment)
                {
                    _flashMessage.Warning("This student is already enrolled in this subject.");

                    model.Subjects = await _subjectRepository.GetComboSubjectsToEnrollAsync(student);
                    return View(model); 
                }

                //criar nova inscrição
                var enrollment = new Enrollment()
                {
                    StudentId = student.Id,
                    EnrollmentDate = model.EnrollmentDate.Value,
                    SubjectId = model.SelectedSubjectId,
                };

                await _enrollmentRepository.CreateAsync(enrollment);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Enrollments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            var enrollment = await _enrollmentRepository.GetEnrollmentWithStudentAndSubjectByIdAsync(id.Value);
            if (enrollment == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(enrollment.StudentId);

            if (student == null)
            {
                _flashMessage.Danger("Not possible to carry on with enrollment");
                return RedirectToAction(nameof(Edit));
            }

            var subjects = await _subjectRepository.GetComboSubjectsToEnrollAsync(student);

            if (subjects == null)
            {
                _flashMessage.Danger("Not possible to carry on with enrollment");
                return RedirectToAction(nameof(Edit));
            }

            var statusList = _studentRepository.GetStudentStatusList();

            if (statusList == null)
            {
                _flashMessage.Warning("No student status loaded, enrollment chages will proceed with same status");
                return RedirectToAction(nameof(Edit));
            }

            var model = _converterHelper.ToEditEnrollmentViewModel(subjects, enrollment, statusList);

            return View(model);
        }

        //POST: Enrollments/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditEnrollmentViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            if (ModelState.IsValid)
            {
                var student = await _studentRepository.GetStudentByFullNameAsync(model.StudentFullName);

                if (student == null)
                {
                    _flashMessage.Danger("Student not found.");
                    return RedirectToAction(nameof(Edit));
                }

                var enrollment = _converterHelper.ToEnrollment(model, student, false);


                try
                {
                    await _enrollmentRepository.UpdateAsync(enrollment);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EnrollmentExists(enrollment.Id))
                    {
                        return new NotFoundViewResult("EnrollmentNotFound");
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

        // GET: Enrollments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            var enrollment = await _enrollmentRepository.GetByIdAsync(id.Value);

            if (enrollment == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            return View(enrollment);
        }

        // POST: Enrollments/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(id);

            if (enrollment == null)
            {
                return new NotFoundViewResult("EnrollmentNotFound");
            }

            try
            {
                await _enrollmentRepository.DeleteAsync(enrollment);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                ViewBag.ErrorTitle = $"Failed to delete enrollment.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }

            
        }

        // chamada AJAX
        [HttpGet]
        public async Task<IActionResult> GetSubjectsForStudent(string studentFullName)
        {
            if (string.IsNullOrWhiteSpace(studentFullName))
            {
                return BadRequest("Full name cannot be empty.");
            }

            var student = await _studentRepository.GetStudentByFullNameAsync(studentFullName);

            if (student == null)
            {
                return NotFound("Student not found with the provided full name.");
            }

            // verificar se aluno está numa turma
            var studentWithSchoolClass = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(student.Id);

            if (studentWithSchoolClass.SchoolClass == null)
            {
                return BadRequest("Enrollment failed, student needs to be enrolled in a school class first.");
            }

            var subjects = await _subjectRepository.GetComboSubjectsToEnrollAsync(studentWithSchoolClass); 

            // Se subjects for null ou apenas tiver "Select a subject...", retornar uma lista vazia
            if (subjects == null || subjects.Count < 1)
            {
                return this.Json(new List<SelectListItem>()); // Retorna uma lista vazia
            }

            // Retorna a lista de SelectListItem como JSON
            return this.Json(subjects);
        }


        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }

        public IActionResult EnrollmentNotFound()
        {
            return View();
        }
    }
}
