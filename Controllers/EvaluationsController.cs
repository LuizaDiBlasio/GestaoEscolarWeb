using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
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

        public EvaluationsController(DataContext context, IEvaluationRepository evaluationRepository, IStudentRepository studentRepository, 
            IFlashMessage flashMessage, ISubjectRepository subjectRepository, IConverterHelper converterHelper, IUserHelper userHelper )
        {
            _context = context;

            _evaluationRepository = evaluationRepository;

            _studentRepository = studentRepository;

            _flashMessage = flashMessage;   

            _subjectRepository = subjectRepository; 

            _converterHelper = converterHelper;

            _userHelper = userHelper;
        }

        // GET: Evaluations
        public async Task<IActionResult> Index()
        {
            return View(await _evaluationRepository.GetEvaluationsWithStudentsAndSubjectsAsync());
        }

       

        //GET do MyGrades (avaliações do estudante)
        public async Task<IActionResult> MyGrades()
        {
            var username = this.User.Identity.Name; //pegar o username

            var user = await _userHelper.GetUserByEmailAsync(username);

            if (user == null)
            {
                _flashMessage.Danger("Not possible to display grades, user not found");
                return View(Enumerable.Empty<Evaluation>());
            }

            //encontrar estudante

            var students = await _studentRepository.GetAllStudentsWithSchoolClassAsync();

            Data.Entities.Student student = students.FirstOrDefault(s => s.Email == username);  //encontrar o student corrspondente ao username

            if (student == null)
            {
                _flashMessage.Danger("Not possible to display grades, student not found");
                return View(Enumerable.Empty<Evaluation>());
            }

            //buscar notas do aluno

            var grades = await _evaluationRepository.GetStudentEvaluationsAsync(student);

            if(!grades.Any())
            {
                _flashMessage.Info("Student doesn't have any grades yet");
                return View(Enumerable.Empty<Evaluation>());
            }

            return View(grades);
        }

        // GET: Evaluations/Create
        public IActionResult Create()
        {
            var model = new CreateEditEvaluationViewModel();

            return View(model);
        }

        // POST: Evaluations/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateEditEvaluationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var student = await _studentRepository.GetStudentWithEnrollmentsAsync(model.StudentId);   //resolver esse metodo 

                if (student == null)
                {
                    _flashMessage.Danger("Not possible the complete evaluation, you need to enroll student first");
                    return View(model);
                }

                //verificar se evaluation já existe pela chave composta
                var existingEvaluation = await _evaluationRepository.ExistingEvaluationAsync(model);

                if (existingEvaluation)
                {
                    _flashMessage.Warning("Evaluation is already registered.");

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

        // GET: Evaluations/Edit/5
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

        //POST: Evaluations/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEditEvaluationViewModel model)
        {

            if (id != model.Id)
            {
                return new NotFoundViewResult("EvaluationNotFound");
            }

            var student = await _studentRepository.GetStudentWithSchoolClassAsync(model.StudentId);

            var evaluation = _converterHelper.ToEvaluation(model, student, false);

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

        // GET: Evaluations/Delete/5
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

        // POST: Evaluations/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(id);

            if (evaluation == null)
            {
                return new NotFoundViewResult("EvaluationNotFound");
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

        private bool EvaluationExists(int id)
        {
            return _context.Evaluations.Any(e => e.Id == id);
        }

        public IActionResult EvaluationNotFound()
        {
            return View();
        }
    }
}
