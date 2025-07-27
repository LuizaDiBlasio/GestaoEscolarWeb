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

        private readonly IEnrollmentRepository _enrollmentRepository;

        public SchoolClassesController(ICourseRepository courseRepository, ISchoolClassRepository schoolClassRepository,
           IFlashMessage flashMessage, IConverterHelper converterHelper, IStudentRepository studentRepository, IEnrollmentRepository enrollmentRepository)
        {
            _courseRepository = courseRepository;

            _schoolClassRepository = schoolClassRepository;

            _flashMessage = flashMessage;

            _converterHelper = converterHelper;

            _studentRepository = studentRepository;

            _enrollmentRepository = enrollmentRepository;

        }

        /// <summary>
        /// Displays a list of all school classes, ordered by school year.
        /// </summary>
        /// <returns>A view displaying the ordered list of school classes.</returns>
        // GET: SchoolClasses
        public async Task<IActionResult> Index()
        {
            var schoolClasses = (await _schoolClassRepository.GetAllSchoolClassesWithCourseAsync()).OrderBy(sc => sc.SchoolYear); //listar todas as turmas

            return View(schoolClasses);
        }


        /// <summary>
        /// Displays a list of all available school classes.
        /// </summary>
        /// <returns>A view displaying list of school classes ordered by Course name.</returns>
        // GET: SchoolClasses
        public async Task<IActionResult> IndexAnonym()
        {
            //listar todas as turmas do ano atual e próximo 

            var schoolClasses = (await _schoolClassRepository.GetAllSchoolClassesWithCourseAsync())
                .Where(sc => sc.SchoolYear == DateTime.Now.Year || sc.SchoolYear == DateTime.Now.Year + 1)
                .OrderBy(sc => sc.Course.Name); 

            return View(schoolClasses);
        }


        /// <summary>
        /// Displays the details of a specific school class, including its associated course and students.
        /// </summary>
        /// <param name="id">The ID of the school class to display details for.</param>
        /// <returns>A view displaying the school class details, or a "School Class Not Found" view if the ID is null or the class does not exist.</returns>
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


        /// <summary>
        /// Displays the view for creating a new school class.
        /// Populates dropdowns for available courses, school years, and shifts.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to create a new school class.</returns>
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


        /// <summary>
        /// Processes the creation of a new school class.
        /// Validates the model, checks for the existence of the selected course.
        /// If successful, it creates the school class and redirects to the Index view.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the new school class details.</param>
        /// <returns>Redirects to the Index view on successful creation, or returns the view with validation errors and repopulated dropdowns.</returns>
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


        /// <summary>
        /// Displays the view for editing an existing school class.
        /// Populates the form with the current school class data and pre-selects dropdown values.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the school class to edit.</param>
        /// <returns>A view with a form to edit the school class, or a "School Class Not Found" view if the ID is null or the class does not exist.</returns>
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


        /// <summary>
        /// Processes the update of an existing school class.
        /// Validates the model and updates the school class in the database.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the school class being edited.</param>
        /// <param name="model">The view model containing the updated school class details.</param>
        /// <returns>Redirects to the Index view on successful update, or returns the view with validation errors and repopulated dropdowns.</returns>
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



        /// <summary>
        /// Confirms and processes the deletion of a school class via AJAX.
        /// </summary>
        /// <param name="id">The ID of the school class to be deleted.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        // POST: Evaluations/DeleteConfirmed/5 
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(id);

            if (schoolClass == null)
            {
                return Json(new { success = false, message = "School class not found." });
            }

            if (schoolClass.Students != null && schoolClass.Students.Any()) //se a turma possuir alunos
            {
                return Json(new { success = false, message = "School class can't be deleted because it has students" });
            }

            //desfazer relação com course
            schoolClass.Course = null;

            try
            {
                await _schoolClassRepository.DeleteAsync(schoolClass);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {

                string errorMessage = "An unexpected database error occurred.";
                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }
                return Json(new { success = false, message = errorMessage });
            }
        }


        /// <summary>
        /// Displays the view for assigning a student to a school class.
        /// Populates a dropdown with available school classes.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to assign a student to a class.</returns>
        //Get AssignToClass
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> AssignToClass()
        {
            var availableSchoolClasses = await _schoolClassRepository.GetComboSchoolClassesAsync();

            var model = new AssignToClassViewModel()
            {
                AvailableSchoolClasses = availableSchoolClasses
            };

            return View(model);
        }


        /// <summary>
        /// Processes the assignment of a student to a school class.
        /// Validates the student and school class, then updates the student's associated class.
        /// Also handles the removal of old enrollments if the student changes courses.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the student ID and the selected school class ID.</param>
        /// <returns>Returns the view with flash messages indicating success or failure, or redirects on success.</returns>
        //Post do AssignToClass
        [Authorize(Roles = "Employee")]
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

                var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(model.SelectedSchoolClassId.Value);

                if (schoolClass == null)
                {
                    _flashMessage.Danger("Unexpected error: not possible to assign student to school class");
                    return View(model);
                }


                //atribuir schoolclass a student
                student.SchoolClass = schoolClass;
                student.SchoolClassId = schoolClass.Id;

                //checar enrollments antigos 

                var newCourseSubjectIds = student.SchoolClass.Course.CourseSubjects.Select(s => s.Id).ToList();

                foreach (var enrollment in student.Enrollments.ToList())
                {
                    if (!newCourseSubjectIds.Contains(enrollment.SubjectId)) // Se a SubjectId do enrollment está na nova turma
                    {
                        try
                        {
                            await _enrollmentRepository.DeleteAsync(enrollment);
                        }
                        catch (Exception ex) 
                        {
                            _flashMessage.Danger("Unexpected error : " + ex.Message);
                            return View(model);
                        }
                    }
                }


                try
                {
                    await _studentRepository.UpdateAsync(student);

                    _flashMessage.Confirmation("Student assigned to class");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger($"Unexpected error: {ex}");
                    return View(model);
                }
            }
            return View(model); 
        }

        /// <summary>
        /// Displays the "School Class Not Found" view when a requested school class cannot be located.
        /// </summary>
        /// <returns>The "School Class Not Found" view.</returns>
        public IActionResult SchoolClassNotFound()
        {
            return View();
        }


    }
}
