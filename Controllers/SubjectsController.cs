using System;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;


namespace GestaoEscolarWeb.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly DataContext _context;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IFlashMessage _flashMessage;
        public SubjectsController(DataContext context, ISubjectRepository subjectRepository, IFlashMessage flashMessage)
        {
            _context = context;
            _subjectRepository = subjectRepository;
            _flashMessage = flashMessage;
        }


        // GET: Subjects
        public IActionResult Index()
        {
            return View(_subjectRepository.GetAll().OrderBy(s => s.Name));
        }


        // GET: Subjects/Create

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subjects/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (ModelState.IsValid)
            {
                var existingSubject = await _subjectRepository.ExistingSubject(subject);

                if (existingSubject)
                {
                    _flashMessage.Warning("Cannot add subject, it already exists.");

                    
                    return View(subject);
                }

                await _subjectRepository.CreateAsync(subject);
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }


        // GET: Subjects/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            var subject = await _subjectRepository.GetByIdAsync(id.Value);
            if (subject == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }
            return View(subject);
        }

        // POST: Subjects/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Id)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _subjectRepository.UpdateAsync(subject);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _subjectRepository.ExistAsync(id))
                    {
                        return new NotFoundViewResult("SubjectNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subject);
        }


        // GET: Subjects/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            var subject = await _subjectRepository.GetByIdAsync(id.Value);
            if (subject == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            return View(subject);   
        }

        // POST: Subjects/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //TODO Resolver o delete em cascata
            var subject = await _subjectRepository.GetSubjectWithCoursesAsync(id);

            if (subject == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            if (subject.SubjectCourses != null)
            {
                subject.SubjectCourses.Clear(); // limpar tabela de join para relações desta subject
            }

            try
            {
                await _subjectRepository.DeleteAsync(subject);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle = $"Failed to delete subject {subject.Name}.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }
        }

        public IActionResult SubjectNotFound()
        {
            return View();  
        }
    }
}
