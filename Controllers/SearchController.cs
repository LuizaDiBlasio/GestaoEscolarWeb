using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;

namespace GestaoEscolarWeb.Controllers
{
    public class SearchController : Controller
    {
        private readonly DataContext _context;

        private readonly ICourseRepository _courseRepository;

        private readonly ISchoolClassRepository _schoolClassRepository;

        private readonly IFlashMessage _flashMessage;

        private readonly IEvaluationRepository _evaluationRepository;

        private readonly IStudentRepository _studentRepository;

        private readonly ISubjectRepository _subjectRepository;

        public SearchController(DataContext context, ICourseRepository courseRepository, ISchoolClassRepository schoolClassRepository,
           IFlashMessage flashMessage, IEvaluationRepository evaluationRepository, IStudentRepository studentRepository, ISubjectRepository subjectRepository)
        {
            _context = context;

            _courseRepository = courseRepository;

            _schoolClassRepository = schoolClassRepository;

            _flashMessage = flashMessage;

            _evaluationRepository = evaluationRepository;

            _studentRepository = studentRepository;

            _subjectRepository = subjectRepository; 

        }

        // GET: Search/Enrollments
        public IActionResult Enrollments()
        {
            var model = new SearchViewModel<Enrollment>();

            return View(model);
        }

        // POST: Search/Enrollments
        [HttpPost]
        public async Task<IActionResult> Enrollments(SearchViewModel<Enrollment> model)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentRepository.GetStudentsByFullNameAsync(model.StudentFullName);

                //se não houver estudantes
                if (students == null || !students.Any())
                {
                    _flashMessage.Danger("Student not found");
                    model.IsSearchSuccessful = false;
                    return View(model);
                }

                if (students.Count() > 1)
                {
                    // Se houver homônimos
                    model.HasHomonyms = true;
                    model.HomonymStudents = students.ToList(); 
                    return View(model); 
                }
                else
                {
                    // Apenas um estudante encontrado
                    var student = students.First();
                    var studentWithEnrollments = await _studentRepository.GetStudentWithEnrollmentsAsync(student.Id);

                    model.Results = studentWithEnrollments.Enrollments;
                    model.IsSearchSuccessful = true;
                    model.StudentFullName = studentWithEnrollments.FullName; 
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet] // GET para mostrar os enrollments
        public async Task<IActionResult> GetEnrollmentsByStudentId(int studentId)
        {
            var student = await _studentRepository.GetStudentWithEnrollmentsAsync(studentId);

            if (student == null)
            {
                _flashMessage.Danger("Student not found.");
                return View();
            }

            var model = new SearchViewModel<Enrollment>
            {
                StudentFullName = student.FullName,
                Results = student.Enrollments,
                IsSearchSuccessful = true
            };

            return View("Enrollments", model); // Retorna a mesma View de inscrições com os resultados
        }

        // GET: Search/SchoolClasses
        public IActionResult SchoolClass()
        {
            var model = new SearchSchoolClassViewModel()
            {
                Students = new List<Data.Entities.Student>(),
                CourseSubjects = new List<Subject>()
            };

            return View(model);
        }

        // POST: Search/SchoolClasses

        [HttpPost]
        public async Task<IActionResult> SchoolClass(SearchSchoolClassViewModel model)
        {
            var schoolClass = await _schoolClassRepository.GetSchoolClassCourseAndStudentsAsync(model.SearchId);

            if (schoolClass == null)
            {
                _flashMessage.Danger("Schooll class not found");
                model.IsSearchSuccessful = false;
                return View(model);
            }

            //passar dados da school class para o model

            model.Shift = schoolClass.Shift;
            model.Course = schoolClass.Course;
            model.Students = schoolClass.Students;
            model.CourseSubjects = schoolClass.Course.CourseSubjects;
            model.SchoolYear = schoolClass.SchoolYear;
            model.IsSearchSuccessful = true;

            return View(model);
        }

        // GET: Search/Grades
        public IActionResult Grades()
        {
            var model = new SearchViewModel<Evaluation>();


            return View(model);
        }


        //POST
        [HttpPost]
        public async Task<IActionResult> Grades(SearchViewModel<Evaluation> model)
        {
            if (ModelState.IsValid)
            {
                var students = await _studentRepository.GetStudentsByFullNameAsync(model.StudentFullName);

                //se não houver estudantes
                if (students == null || !students.Any())
                {
                    _flashMessage.Danger("Student not found");
                    model.IsSearchSuccessful = false;
                    return View(model);
                }

                if (students.Count() > 1)
                {
                    // Se houver homônimos
                    model.HasHomonyms = true;
                    model.HomonymStudents = students.ToList();
                    return View(model);
                }
                else
                {
                    // Apenas um estudante encontrado
                    var student = students.First();
                    var studentWithEvaluations = await _studentRepository.GetStudentWithEvaluationsAsync(student.Id);

                    model.Results = studentWithEvaluations.Evaluations;
                    model.IsSearchSuccessful = true;
                    model.StudentFullName = studentWithEvaluations.FullName;
                    return View(model);
                }
            }
            return View(model);

        }

        [HttpGet] // GET para mostrar as evaluations
        public async Task<IActionResult> GetEvaluationsByStudentId(int studentId)
        {
            var student = await _studentRepository.GetStudentWithEvaluationsAsync(studentId);

            if (student == null)
            {
                _flashMessage.Danger("Student not found.");
                return View();
            }

            var model = new SearchViewModel<Evaluation>
            {
                StudentFullName = student.FullName,
                Results = student.Evaluations,
                IsSearchSuccessful = true
            };

            return View("Grades", model); // Retorna a mesma View de inscrições com os resultados
        }


        // GET: Search/Students
        public IActionResult Student()
        {
            var model = new SearchStudentViewModel();

            return View(model);
        }

        // POST: Search/Student
        [HttpPost]
        public async Task<IActionResult> Student(SearchStudentViewModel model)
        {
            var students = await _studentRepository.GetStudentsByFullNameAsync(model.SearchFullName);

            //se não houver estudantes
            if (students == null || !students.Any())
            {
                _flashMessage.Danger("Student not found");
                model.IsSearchSuccessful = false;
                return View(model);
            }

            if (students.Count() > 1)
            {
                // Se houver homônimos
                model.HasHomonyms = true;
                model.HomonymStudents = students.ToList();
                return View(model);
            }
            else
            {
                // Apenas um estudante encontrado
                var student = students.First();

                model.BirthDate = student.BirthDate;
                model.PhoneNumber = student.PhoneNumber;
                model.Email = student.Email;
                model.Id = student.Id;
                model.Address = student.Address;
                model.ProfileImageId = student.ProfileImageId;
                model.SchoolClass = student.SchoolClass == null
                            ? "A student not enrolled in any school class"
                            : student.SchoolClass.Id.ToString();
                model.UserStudentId = student.UserStudentId;
                model.IsSearchSuccessful = true;

                return View(model);
            }
        }


        // GET: Search/Courses
        public IActionResult Courses()
        {
            var model = new SearchCourseViewModel();
            
            return View(model);
        }


        // POST: Search/Courses
        [HttpPost]
        public async  Task<IActionResult> Courses(SearchCourseViewModel model)
        {
            var course = await _courseRepository.GetCourseSubjectsAndSchoolClassesByIdAsync(model.SearchId);

            if(course == null)
            {
                _flashMessage.Danger("Course not found");
                model.IsSearchSuccessful = false;
                return View(model);
            } 

            var courseStudents = await _courseRepository.GetStudentsFromCourseAsync(model.SearchId);

            model.Id = course.Id;
            model.Name = course.Name;       
            model.CourseStudents = (ICollection<Data.Entities.Student>)courseStudents;
            model.IsSearchSuccessful = true;
            model.StartDate = course.StartDate;
            model.EndDate = course.EndDate; 
            model.SchoolClasses = course.SchoolClasses;
            model.CourseSubjects = course.CourseSubjects;   


            return View(model);
        }



        [HttpGet] // GET para mostrar o student
        public async Task<IActionResult> GetStudentById(int studentId)
        {
            var student = await _studentRepository.GetStudentWithEvaluationsAsync(studentId);

            if (student == null)
            {
                _flashMessage.Danger("Student not found.");
                return View();
            }

            var model = new SearchStudentViewModel()
            {
                BirthDate = student.BirthDate,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                Id = student.Id,
                Address = student.Address,
                ProfileImageId = student.ProfileImageId,
                SchoolClass = student.SchoolClass == null
                            ? "A student not enrolled in any school class"
                            : student.SchoolClass.Id.ToString(),
                UserStudentId = student.UserStudentId,
                IsSearchSuccessful = true,
            };

            return View("Student", model); // Retorna a mesma View de inscrições com os resultados
        }


        //GET
        public IActionResult Subjects()
        {
            var model = new SearchSubjectViewModel();

            return View(model);
        }


        //POST
        [HttpPost]
        public async Task<IActionResult> Subjects(SearchSubjectViewModel model)
        {
            var subject = await _subjectRepository.GetSubjectByNameAsync(model.SearchSubjectName);  
            
            if(subject == null)
            {
                _flashMessage.Danger("Subject not found");
                model.IsSearchSuccessful = false;
                return View(model);
            }

            //atribuir propriedades ao model
 
            model.IsSearchSuccessful=true;
            model.Name = subject.Name;
            model.SubjectCourses = subject.SubjectCourses;
            model.CreditHours = subject.CreditHours;    
            model.Id = subject.Id;

            return View(model);
        }

    }
}
