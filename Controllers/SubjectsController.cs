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
        public SubjectsController(ISubjectRepository subjectRepository, IFlashMessage flashMessage)
        {
            _subjectRepository = subjectRepository;

            _flashMessage = flashMessage;
        }


        /// <summary>
        /// Displays a list of all subjects, ordered by name.
        /// </summary>
        /// <returns>A view displaying the ordered list of subjects.</returns>
        // GET: Subjects
        public IActionResult Index()
        {
            return View(_subjectRepository.GetAll().OrderBy(s => s.Name));
        }


        /// <summary>
        /// Displays the view for creating a new subject.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <returns>A view with a form to create a new subject.</returns>
        // GET: Subjects/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }


        /// <summary>
        /// Processes the creation of a new subject.
        /// Validates the model and checks if a subject with the same name already exists.
        /// If successful, it creates the subject and redirects to the Index view.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="subject">The subject object containing the new subject details.</param>
        /// <returns>Redirects to the Index view on successful creation, or returns the view with validation errors and flash messages.</returns>
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


        /// <summary>
        /// Displays the view for editing an existing subject.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the subject to edit.</param>
        /// <returns>A view with a form to edit the subject, pre-populated with existing data, or a "Subject Not Found" view if the ID is null or the subject does not exist.</returns>
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


        /// <summary>
        /// Processes the update of an existing subject.
        /// Validates the model and handles concurrency exceptions during database updates.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the subject being edited.</param>
        /// <param name="subject">The subject object containing the updated details.</param>
        /// <returns>Redirects to the Index view on successful update, or returns the view with validation errors.</returns>
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


        /// <summary>
        /// Displays the confirmation view for deleting a subject.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the subject to delete.</param>
        /// <returns>A view confirming the deletion, or a "Subject Not Found" view if the ID is null or the subject does not exist.</returns>
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


        /// <summary>
        /// Confirms and processes the deletion of a subject.
        /// Clears any associated courses before attempting to delete the subject.
        /// Handles database exceptions during deletion.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the subject to be deleted.</param>
        /// <returns>Redirects to the Index view on successful deletion, or returns an error view if deletion fails.</returns>
        // POST: Subjects/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
          
            var subject = await _subjectRepository.GetSubjectWithCoursesAsync(id);

            if (subject == null)
            {
                return new NotFoundViewResult("SubjectNotFound");
            }

            if (subject.SubjectCourses != null)
            {
                _flashMessage.Danger("Subject cannot be deleted, it belongs to a course");

                return View();
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


        /// <summary>
        /// Displays the "Subject Not Found" view when a requested subject cannot be located.
        /// </summary>
        /// <returns>The "Subject Not Found" view.</returns>
        public IActionResult SubjectNotFound()
        {
            return View();  
        }
    }
}
