using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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


        private readonly IStudentRepository _studentRepository;

        private readonly ISubjectRepository _subjectRepository;

        private readonly IEnrollmentRepository _enrollmentRepository;

        private readonly IUserHelper _userHelper;

        private readonly HttpClient _httpClient;

        public SearchController(DataContext context, ICourseRepository courseRepository, ISchoolClassRepository schoolClassRepository,
           IFlashMessage flashMessage, IStudentRepository studentRepository, IUserHelper userHelper,
           ISubjectRepository subjectRepository, IEnrollmentRepository enrollmentRepository, HttpClient httpClient)
        {
            _context = context;

            _courseRepository = courseRepository;

            _schoolClassRepository = schoolClassRepository;

            _flashMessage = flashMessage;

            _studentRepository = studentRepository;

            _subjectRepository = subjectRepository;
            
            _enrollmentRepository = enrollmentRepository;

            _userHelper = userHelper;

            _httpClient = httpClient;   

        }

        /// <summary>
        /// Displays the search form for enrollments.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to search for enrollments.</returns>
        // GET: Search/Enrollments
        [Authorize(Roles = "Employee")]
        public IActionResult Enrollments()
        {
            var model = new SearchViewModel<Enrollment>();

            return View(model);
        }


        /// <summary>
        /// Processes the search for enrollments based on a student's full name.
        /// If multiple students with the same name are found, it presents them as homonyms.
        /// Otherwise, it retrieves and displays the enrollments for the unique student,
        /// including their status.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the student's full name for the search.</param>
        /// <returns>A view displaying the search results, homonyms, or an error message.</returns>
        // POST: Search/Enrollments
        [Authorize(Roles = "Employee")]
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

                    //atribuir status para cada enrollment
                    foreach (var enrollment in studentWithEnrollments.Enrollments)
                    {
                        enrollment.StudentStatus = await _enrollmentRepository.GetStudentStatusAsync(enrollment);
                    }

                    model.Results = studentWithEnrollments.Enrollments;
                    model.IsSearchSuccessful = true;
                    model.StudentFullName = studentWithEnrollments.FullName; 
                    return View(model);
                }
            }
            return View(model);
        }


        /// <summary>
        /// Retrieves and displays enrollments for a specific student identified by their ID.
        /// This is typically used when a student has been selected from a list of homonyms.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="studentId">The ID of the student whose enrollments are to be retrieved.</param>
        /// <returns>A view displaying the student's enrollments or an error message if the student is not found.</returns>
        [Authorize(Roles = "Employee")]
        [HttpGet] // GET para mostrar os enrollments
        public async Task<IActionResult> GetEnrollmentsByStudentId(int studentId)
        {
            var student = await _studentRepository.GetStudentWithEnrollmentsAsync(studentId);

            foreach (var enrollment in student.Enrollments)
            {
                enrollment.StudentStatus = await _enrollmentRepository.GetStudentStatusAsync(enrollment);
            }

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



        /// <summary>
        /// Displays the search form for school classes.
        /// Initializes the model with an empty list of students.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to search for school classes.</returns>
        // GET: Search/SchoolClasses
        [Authorize(Roles = "Employee")]
        public IActionResult SchoolClass()
        {
            var model = new SearchSchoolClassViewModel()
            {
                Students = new List<Data.Entities.Student>()
            };

            return View(model);
        }


        /// <summary>
        /// Processes the search for a school class by its ID and retrieves its associated students via an API call.
        /// It validates the user's authorization token and handles various API response statuses.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the school class ID for the search.</param>
        /// <returns>A view displaying the school class details and its students, or an error/redirection based on the outcome.</returns>
        // POST: Search/SchoolClasses
        [Authorize(Roles = "Employee")]
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
            model.SchoolYear = schoolClass.SchoolYear;

            if (!ModelState.IsValid)
            {
                model.IsSearchSuccessful = false;
                _flashMessage.Danger("Invalid school class Id"); // Mensagem para validação
                return View(model);
            }

            var jwtToken = HttpContext.Session.GetString("JwtToken"); //Pegar o token armazenado em cookies na Session

            if (string.IsNullOrEmpty(jwtToken)) //se não houve token, o login não foi feito por um user no role autorizado
            {
                //checar se é um employee logado com cookies 

                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                var isEmployee = await _userHelper.IsUserInRoleAsync(user, "Employee");

                if (isEmployee)
                {
                    _flashMessage.Danger("Access to this data has expired. Please, logout and login again.");
                    return View(model);
                }

                //caso nãop seja employee:
                _flashMessage.Danger("Anauthorized access, only employees have access to this information ");
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken); //criar cabeçalho de autorização com o token para as requisições http

                string apiUrl = $"https://localhost:44385/api/SchoolClassStudents/{model.SearchId}"; //definir o url da api com o id de busca

                var response = await _httpClient.GetAsync(apiUrl); //busca conteudo na api

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(); //leitura do conteudo em string

                    // Desserializa  para uma lista de estudantes
                    var studentsFromApi = JsonSerializer.Deserialize<ICollection<Student>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Atribui a lista de estudantes ao ViewModel
                    model.Students = studentsFromApi;

                    model.IsSearchSuccessful = true; // Indica que a busca por alunos foi bem-sucedida
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)// Caso a requisição não tenha sido autorizada
                {
                    HttpContext.Session.Remove("JwtToken"); //limpar token invalido/expirado da sessão
                    _flashMessage.Danger("Session expired, please login again");
                    return RedirectToAction("Login", "Account", new { ReturnUrl = Url.Action("SchoolClass", "Search", new { id = model.SearchId }) });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound) // se o retorno da api for not found
                {
                    _flashMessage.Warning($"School class Id {model.SearchId} was not found or has no students.");
                    model.IsSearchSuccessful = false;
                }
                else
                {
                    //em caso de erro pegar a resposta da api e mostrar
                    var errorContent = await response.Content.ReadAsStringAsync(); 
                    _flashMessage.Danger($"Error loading: {response.StatusCode} - {errorContent}");
                    model.IsSearchSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _flashMessage.Danger($"An unexpected error occurred: {ex.Message}");
                model.IsSearchSuccessful = false;
            }

            return View(model);
        }


        /// <summary>
        /// Displays the search form for student grades (evaluations).
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to search for grades.</returns>
        // GET: Search/Grades
        [Authorize(Roles = "Employee")]
        public IActionResult Grades()
        {
            var model = new SearchViewModel<Evaluation>();


            return View(model);
        }


        /// <summary>
        /// Processes the search for student grades (evaluations) based on a student's full name.
        /// If multiple students with the same name are found, it presents them as homonyms.
        /// Otherwise, it retrieves and displays the evaluations for the unique student.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the student's full name for the search.</param>
        /// <returns>A view displaying the search results, homonyms, or an error message.</returns>
        //POST
        [Authorize(Roles = "Employee")]
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


        /// <summary>
        /// Retrieves and displays evaluations (grades) for a specific student identified by their ID.
        /// This is typically used when a student has been selected from a list of homonyms.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="studentId">The ID of the student whose evaluations are to be retrieved.</param>
        /// <returns>A view displaying the student's evaluations or an error message if the student is not found.</returns>
        [Authorize(Roles = "Employee")]
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


        /// <summary>
        /// Displays the search form for student profiles.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view with a form to search for students.</returns>
        // GET: Search/Students
        [Authorize(Roles = "Employee")]
        public IActionResult Student()
        {
            var model = new SearchStudentViewModel();

            return View(model);
        }


        /// <summary>
        /// Processes the search for a student profile based on their full name.
        /// If multiple students with the same name are found, it presents them as homonyms.
        /// Otherwise, it retrieves and displays the full profile details for the unique student.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="model">The view model containing the student's full name for the search.</param>
        /// <returns>A view displaying the student's profile, homonyms, or an error message.</returns>
        // POST: Search/Student
        [Authorize(Roles = "Employee")]
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


        /// <summary>
        /// Displays the search form for courses.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <returns>A view with a form to search for courses.</returns>
        [Authorize(Roles = "Admin")]
        public IActionResult Courses()
        {
            var model = new SearchCourseViewModel();
            
            return View(model);
        }


        /// <summary>
        /// Processes the search for a course by its ID.
        /// Retrieves and displays details of the course, including its subjects, school classes, and enrolled students.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="model">The view model containing the course ID for the search.</param>
        /// <returns>A view displaying the course details or an error message if the course is not found.</returns>
        // POST: Search/Courses
        [Authorize(Roles = "Admin")]
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
            model.SchoolClasses = course.SchoolClasses;
            model.CourseSubjects = course.CourseSubjects;   


            return View(model);
        }


        /// <summary>
        /// Retrieves and displays the profile of a specific student identified by their ID.
        /// This is typically used when a student has been selected from a list of homonyms or for quick access.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="studentId">The ID of the student whose profile is to be retrieved.</param>
        /// <returns>A view displaying the student's profile or an error message if the student is not found.</returns>
        //Ajax
        [Authorize(Roles = "Employee")]
        [HttpGet] // GET para mostrar o profile do student
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


        /// <summary>
        /// Displays the search form for subjects.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <returns>A view with a form to search for subjects.</returns>
        //GET
        [Authorize(Roles = "Admin")]
        public IActionResult Subjects()
        {
            var model = new SearchSubjectViewModel();

            return View(model);
        }


        /// <summary>
        /// Processes the search for a subject by its name.
        /// Retrieves and displays details of the subject, including its associated courses and credit hours.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="model">The view model containing the subject name for the search.</param>
        /// <returns>A view displaying the subject details or an error message if the subject is not found.</returns>
        //POST
        [Authorize(Roles = "Admin")]
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
