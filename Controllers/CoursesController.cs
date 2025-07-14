using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;


namespace GestaoEscolarWeb.Controllers
{
    public class CoursesController : Controller
    {
        private readonly DataContext _context;

        private readonly ISubjectRepository _subjectRepository;

        private readonly ICourseRepository _courseRepository;

        private readonly IConverterHelper _converterHelper;

        private readonly IFlashMessage _flashMessage;

        public CoursesController(DataContext context, ISubjectRepository subjectRepository, ICourseRepository courseRepository, 
            IConverterHelper converterHelper, IFlashMessage flashMessage)
        {
            _context = context;

            _subjectRepository = subjectRepository;

            _courseRepository = courseRepository;

            _converterHelper = converterHelper;

            _flashMessage = flashMessage;
        }


        /// <summary>
        /// Displays a list of all courses, ordered by name.
        /// </summary>
        /// <returns>A view displaying the list of courses.</returns>
        // GET: Courses
        public async Task<IActionResult> Index()
        {
            return  View(_courseRepository.GetAll().OrderBy(c => c.Name));
        }


        /// <summary>
        /// Displays the details of a specific course, including its associated subjects and school classes.
        /// </summary>
        /// <param name="id">The ID of the course to display details for.</param>
        /// <returns>A view displaying the course details, or a "Course Not Found" view if the course does not exist.</returns>
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            var course = await _courseRepository.GetCourseSubjectsAndSchoolClassesByIdAsync(id.Value);
            if (course == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            return View(course);
        }


        /// <summary>
        /// Displays the view for creating a new course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <returns>A view with a form to create a new course, pre-populated with available subjects.</returns>
        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new CourseViewModel();

            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();

            return View(model); // Passar a ViewModel populada
        }


        /// <summary>
        /// Processes the creation of a new course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="model">The view model containing the course details and selected subjects.</param>
        /// <returns>Redirects to the Index view on successful creation, or returns the view with validation errors.</returns>
        // POST: Courses/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (ModelState.IsValid) 
            {
                // Criar um curso
                var course = new Course
                {
                    Name = model.Name,
                    CourseSubjects = new List<Subject>() // Inicializar a lista de Subjects
                };

                // Adicionar as Subjects selecionadas
                if (model.SelectedSubjectIds != null && model.SelectedSubjectIds.Any())
                {
                    foreach (var subjectId in model.SelectedSubjectIds)
                    {
                        var subject = await _subjectRepository.GetByIdAsync(subjectId);
                        if (subject != null)
                        {
                            course.CourseSubjects.Add(subject); // em memória
                        }
                    }
                }

                    await _courseRepository.CreateAsync(course); //CreateAsync add na database e salva
                    return RedirectToAction(nameof(Index));

            }


            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();

            return View(model);
        }


        /// <summary>
        /// Displays the view for editing an existing course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the course to edit.</param>
        /// <returns>A view with a form to edit the course, pre-populated with existing data and available subjects.</returns>
        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            var course = await _courseRepository.GetCourseSubjectsAndSchoolClassesByIdAsync(id.Value);
            if (course == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            var model = _converterHelper.ToCourseViewModel(course); //converter para model

            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync(); //popular lista de seleção de subjects

            return View(model);
        }


        /// <summary>
        /// Processes the update of an existing course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the course being edited.</param>
        /// <param name="model">The view model containing the updated course details and selected subjects.</param>
        /// <returns>Redirects to the Index view on successful update, or returns the view with validation errors.</returns>
        // POST: Courses/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, CourseViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            if (ModelState.IsValid)
            {
                var course = await _courseRepository.GetCourseSubjectsAndSchoolClassesByIdAsync(id); //buscar curso com subjects

                //atualizar propriedades
                course.Name = model.Name;

                // atualizar listas de subjects
                var selectedSubjectIdsSet = new List<int>(model.SelectedSubjectIds ?? new List<int>());

                //Remover Subjects que foram desmarcados
                foreach (var subjectToRemove in course.CourseSubjects.ToList()) // .ToList() para evitar "Collection was modified"
                {
                    if (!selectedSubjectIdsSet.Contains(subjectToRemove.Id))
                    {
                        course.CourseSubjects.Remove(subjectToRemove);
                    }
                }

               //Adicionar Subjects que foram marcados
                var subjectIdsInDb = new List<int>(course.CourseSubjects.Select(s => s.Id));

                foreach (var selectedId in selectedSubjectIdsSet)
                {
                    if (!subjectIdsInDb.Contains(selectedId))
                    {
                        // Buscar o Subject existente do banco de dados 
                        var subjectToAdd = await _subjectRepository.GetByIdAsync(selectedId);
                        if (subjectToAdd != null)
                        {
                            course.CourseSubjects.Add(subjectToAdd);
                        }
                    }
                }

                try
                {
                    await _courseRepository.UpdateAsync(course);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _courseRepository.ExistAsync(id))
                    {
                        return NotFound(); //TODO fazer notfound personalizado
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Se houver erros de validação, re-popular SubjectsToSelect antes de retornar a view
            model.SubjectsToSelect = await _subjectRepository.GetSubjectsToSelectAsync();
            return View(model);
           
        }


        /// <summary>
        /// Displays the confirmation view for deleting a course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the course to delete.</param>
        /// <returns>A view confirming the deletion, or a "Course Not Found" view if the course does not exist.</returns>
        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        { 
            if (id == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            var course = await _courseRepository?.GetByIdAsync(id.Value);
            if (course == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }

            return View(course);
        }


        /// <summary>
        /// Confirms and processes the deletion of a course.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the course to be deleted.</param>
        /// <returns>Redirects to the Index view on successful deletion, or returns an error view if deletion fails due to related entities.</returns>
        // POST: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetCourseSubjectsAndSchoolClassesByIdAsync(id);

            if (course == null)
            {
                return new NotFoundViewResult("CourseNotFound");
            }
            if (course.SchoolClasses != null && course.SchoolClasses.Any())
            {
                _flashMessage.Danger($"{course.Name} can't be deleted, school classes belong to this course.");
                return RedirectToAction(nameof(Delete), new { id = course.Id });
            }

            if (course.CourseSubjects != null)
            {
                course.CourseSubjects.Clear(); // limpar tabela de join para relações deste course
            }

            try
            {
                await _courseRepository.DeleteAsync(course);

                return RedirectToAction(nameof(Index));
            }
            catch(Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                ViewBag.ErrorTitle = $"Failed to delete course '{course.Name}'.";

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
        /// Displays the "Course Not Found" view when a requested course does not exist.
        /// </summary>
        /// <returns>The "Course Not Found" view.</returns>
        public IActionResult CourseNotFound()
        {
            return View();  
        }

    }
}
