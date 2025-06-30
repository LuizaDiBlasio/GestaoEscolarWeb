using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;

namespace GestaoEscolarWeb.Controllers
{
    public class SchoolClassesController : Controller
    {
        private readonly DataContext _context;

        private readonly ICourseRepository _courseRepository;

        private readonly ISchoolClassRepository _schoolClassRepository;

        private readonly IFlashMessage _flashMessage;

        private readonly IConverterHelper _converterHelper;

        public SchoolClassesController(DataContext context, ICourseRepository courseRepository, ISchoolClassRepository schoolClassRepository,
           IFlashMessage flashMessage, IConverterHelper converterHelper)
        {
            _context = context;
            _courseRepository = courseRepository;
            _schoolClassRepository = schoolClassRepository;
            _flashMessage = flashMessage;
            _converterHelper = converterHelper;

        }

        // GET: SchoolClasses
        public async Task<IActionResult> Index()
        {
            var schoolClasses = (await _schoolClassRepository.GetAllSchoolClassesWithCourseAsync()).OrderBy(sc => sc.SchoolYear); //listar todas as turmas

            return View(schoolClasses);
        }

        // GET: SchoolClasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id.Value); //encontrar a turma e mandar para a view

            if (schoolClass == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            return View(schoolClass);
        }

        // GET: SchoolClasses/Create
        public IActionResult Create()
        {
            var model = new CreateEditSchoolClassViewModel
            {
                AvailableCourses = _courseRepository.GetComboCourses(), //mandar a combo preenchida com cursos
                SchoolYears = _schoolClassRepository.GetComboSchoolYears(), //mandar a combo preenchida com anos 
                Shifts = _schoolClassRepository.GetComboShifts()
            };
            return View(model);
        }

        //POST: SchoolClasses/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateEditSchoolClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                //buscar todos os cursos 
                var courses = _courseRepository.GetAll();

                if (courses == null)
                {
                    return new NotFoundViewResult("SchoolClassNotFound");
                }

                //ver se course selecionado existe
                Course schoolClassCourse = courses.FirstOrDefault(c => c.Id == model.SelectedCourseId);

                if (schoolClassCourse == null)
                {
                    ModelState.AddModelError("SelectedCourseId", "The selected course does not exist.");
                }

                //buscar shift selecionado - converter de selected Item id para string
                var shift = _converterHelper.ToShift(model);

                if (shift == null)
                {
                    ModelState.AddModelError(string.Empty, "It was not possible to choose a shift");

                    // Repopular combos antes de retornar a view
                    model.AvailableCourses = _courseRepository.GetComboCourses();
                    model.SchoolYears = _schoolClassRepository.GetComboSchoolYears();
                    model.Shifts = _schoolClassRepository.GetComboShifts();

                    return View(model);
                }

                // Criar um curso
                var schoolClass = new SchoolClass
                {
                    CourseId = model.SelectedCourseId,
                    SchoolYear = model.SelectedYear,
                    Shift = shift
                };

                await _schoolClassRepository.CreateAsync(schoolClass); //CreateAsync add na database e salva
                return RedirectToAction(nameof(Index));
            }

            // Repopular combos antes de retornar a view
            model.AvailableCourses = _courseRepository.GetComboCourses();
            model.SchoolYears = _schoolClassRepository.GetComboSchoolYears();
            model.Shifts = _schoolClassRepository.GetComboShifts();

            return View(model);
        }

        // GET: SchoolClasses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id.Value);
            if (schoolClass == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            //converter id(int) - turno (string)
            var selectedId = _converterHelper.ToSelectedId(schoolClass);

            var model = new CreateEditSchoolClassViewModel
            {

                //mandar listas das combos e pripriedades selecionadas
                AvailableCourses = _courseRepository.GetComboCourses(),
                SelectedCourseId = schoolClass.CourseId,

                SchoolYears = _schoolClassRepository.GetComboSchoolYears(),
                SelectedYear = schoolClass.SchoolYear,

                Shifts = _schoolClassRepository.GetComboShifts(),
                SelectedShiftId = selectedId
            };

            return View(model);
        }

        //POST: SchoolClasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEditSchoolClassViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            if (ModelState.IsValid)
            {
                var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id);

                if (schoolClass == null)
                {
                    return new NotFoundViewResult("SchoolClassNotFound");
                }

                //buscar shift selecionado - converter de selected Item id para string
                var shift = _converterHelper.ToShift(model);

                if (shift == null)
                {
                    ModelState.AddModelError(string.Empty, "It was not possible to choose a shift");

                    // Repopular combos antes de retornar a view
                    model.AvailableCourses = _courseRepository.GetComboCourses();
                    model.SchoolYears = _schoolClassRepository.GetComboSchoolYears();
                    model.Shifts = _schoolClassRepository.GetComboShifts();

                    return View(model);
                }

                //Fazer modificações
                schoolClass.CourseId = model.SelectedCourseId;
                schoolClass.Shift = shift;
                schoolClass.SchoolYear = model.SelectedYear;

                try
                {
                    await _schoolClassRepository.UpdateAsync(schoolClass);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _schoolClassRepository.ExistAsync(id))
                    {
                        return new NotFoundViewResult("SchoolClassNotFound"); ;
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopular combos antes de retornar a view
            model.AvailableCourses = _courseRepository.GetComboCourses();
            model.SchoolYears = _schoolClassRepository.GetComboSchoolYears();
            model.Shifts = _schoolClassRepository.GetComboShifts();

            return View(model);
        }

        // GET: SchoolClasses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id.Value);

            if (schoolClass == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            return View(schoolClass);
        }

        // POST: SchoolClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id);

            if (schoolClass == null)
            {
                return new NotFoundViewResult("SchoolClassNotFound");
            }

            if (schoolClass.Students != null && schoolClass.Students.Any()) //se a turma possuir alunos
            {
                _flashMessage.Danger($"School class can't be deleted because it has students");
                return RedirectToAction(nameof(Delete), new { id = schoolClass.Id });
            }

            //desfazer relação com course
            schoolClass.Course = null;

            //deletar
            try
            {
                await _schoolClassRepository.DeleteAsync(schoolClass);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                ViewBag.ErrorTitle = $"Failed to delete school class '{schoolClass.CourseYearShift}'.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }


        }

        public IActionResult SchoolClassNotFound()
        {
            return View();
        }


    }
}
