using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Tls;
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

        private readonly IStudentRepository _studentRepository;

        public SchoolClassesController(DataContext context, ICourseRepository courseRepository, ISchoolClassRepository schoolClassRepository,
           IFlashMessage flashMessage, IConverterHelper converterHelper, IStudentRepository studentRepository)
        {
            _context = context;
            _courseRepository = courseRepository;
            _schoolClassRepository = schoolClassRepository;
            _flashMessage = flashMessage;
            _converterHelper = converterHelper;
            _studentRepository = studentRepository;

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
        [Authorize(Roles = "Employee")]
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
        [Authorize(Roles = "Employee")]
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
        [Authorize(Roles = "Employee")]
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
        [Authorize(Roles = "Employee")]
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
        [Authorize(Roles = "Employee")]
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
        [Authorize(Roles = "Employee")]
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
                return View("Delete", schoolClass);
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

                ViewBag.ErrorTitle = $"Failed to delete school class.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }


        }

        //Get AssignToClass
        public async Task<IActionResult> AssignToClass()
        {
            var availableSchoolClasses = await _schoolClassRepository.GetComboSchoolClassesAsync();

            var model = new AssignToClassViewModel()
            {
                AvailableSchoolClasses = availableSchoolClasses
            };

            return View(model);
        }


        //Post do AssignToClass
        [HttpPost]
        [Route("SchoolClasses/AssignToClass")]
        public async Task<IActionResult> AssignToClass(AssignToClassViewModel model)
        {
            if(ModelState.IsValid)
            {
                var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(model.StudentId.Value);

                if (student == null)
                {
                    _flashMessage.Danger("Unexpected error: not possible to assign student to school class");
                    return View(model);
                }

                var schoolClass = await _schoolClassRepository.GetByIdAsync(model.SelectedSchoolClassId.Value);

                if (schoolClass == null)
                {
                    _flashMessage.Danger("Unexpected error: not possible to assign student to school class");
                    return View(model);
                }

                //atribuir schoolclass a student
                student.SchoolClass = schoolClass;
                student.SchoolClassId = schoolClass.Id;

                

                try
                {
                    await _studentRepository.UpdateAsync(student);
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger($"Unexpected error: {ex}");
                    return View(model);
                }
            }
            return View(model); 
        }

        public IActionResult SchoolClassNotFound()
        {
            return View();
        }


    }
}
