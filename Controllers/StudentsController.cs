using System;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;


namespace GestaoEscolarWeb.Controllers
{
    public class StudentsController : Controller
    {
        private readonly DataContext _context;

        private readonly IUserHelper _userHelper;

        private readonly IBlobHelper _blobHelper;

        private readonly IStudentRepository _studentRepository;

        private readonly IFlashMessage _flashMessage;

        private readonly IConverterHelper _converterHelper;

        private readonly IMailHelper _mailHelper;

        private readonly ISchoolClassRepository _schoolClassRepository; 

        private readonly IEnrollmentRepository _enrollmentRepository;

       

        public StudentsController(IUserHelper userHelper, IBlobHelper blobHelper,
            IStudentRepository studentRepository, IFlashMessage flashMessage, IConverterHelper converterHelper,
            IMailHelper mailHelper, ISchoolClassRepository schoolClassRepository, IEnrollmentRepository enrollmentRepository)
        {
            _userHelper = userHelper;

            _blobHelper = blobHelper;

            _studentRepository = studentRepository;

            _flashMessage = flashMessage;  
            
            _schoolClassRepository = schoolClassRepository;
            
            _converterHelper = converterHelper; 

            _mailHelper = mailHelper; 

            _enrollmentRepository = enrollmentRepository;
              
        }

        /// <summary>
        /// Displays a list of all students with their associated school classes.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <returns>A view containing a list of students.</returns>
        [Authorize(Roles = "Employee")]
        // GET: Students
        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllStudentsWithSchoolClassAsync();
            return View(students);
        }


        /// <summary>
        /// Displays the profile of the currently logged-in student.
        /// Only accessible by users with the "Student" role.
        /// </summary>
        /// <returns>A view displaying the student's profile information.</returns>
        //Get do MyProfile
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyProfile()
        {
            var username = this.User.Identity.Name; //pegar o username

            var user = await _userHelper.GetUserByEmailAsync(username);

            if (user == null)
            {
                _flashMessage.Danger("User not found");
                return RedirectToAction(nameof(MyProfile));
            }

            //encontrar estudante

            var students = await _studentRepository.GetAllStudentsWithSchoolClassAsync();

            Data.Entities.Student student = students.FirstOrDefault(s => s.Email == username);  //encontrar o student corrspondente ao username

            if (student == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            //converter student para model
            var model = _converterHelper.ToMyProfileViewModel(student); 
            
            //mandar model para a view
            return View(model);
        }


        /// <summary>
        /// Handles the POST request for updating the currently logged-in student's profile.
        /// Allows students to update their personal information and profile image.
        /// Only accessible by users with the "Student" role.
        /// </summary>
        /// <param name="id">The ID of the student to update.</param>
        /// <param name="model">The view model containing the updated student information.</param>
        /// <returns>Redirects to the MyProfile action on successful update, or returns the view with errors.</returns>
        // POST: Students/MyProfile/5
        [Authorize(Roles = "Student")]
        [HttpPost]
        public async Task<IActionResult> MyProfile(int id, MyProfileViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Guid profileImageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                    if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                    {
                        profileImageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                    }

                    var student = _converterHelper.FromMyProfileToStudent(model, false, profileImageId);

                    // Atribuir nova guid ou guid antiga
                    student.ProfileImageId = profileImageId != Guid.Empty ? profileImageId : model.ProfileImageId;

                    await _studentRepository.UpdateAsync(student); //atualizar student

                    //atualizar user 

                    var user = await _userHelper.GetUserByEmailAsync(model.Email);  

                    //adicionar propriedades modificadas

                    user.BirthDate = model.BirthDate;   
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;   

                    await _userHelper.UpdateUserAsync(user); //atualizar user correspondente à student

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(model.Id))
                    {
                        return new NotFoundViewResult("StudentNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }

                _flashMessage.Confirmation($"Your profile has been updated successfully.");

                return RedirectToAction(nameof(MyProfile), model);
            }
            return View(model);
        }


        /// <summary>
        /// Displays the detailed information of a specific student.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="id">The ID of the student to display.</param>
        /// <returns>A view displaying the student's details, or a "StudentNotFound" view if the student is not found.</returns>
        // GET: Students/Details/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(id.Value);
            if (student == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            return View(student);
        }


        /// <summary>
        /// Displays the form for creating a new student.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <returns>A view with the student creation form, populated with available school classes.</returns>
        // GET: Students/Create
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Create()
        {
            //buscar a lista para a comboBox

            var availableSchoolClasses = await _schoolClassRepository.GetComboSchoolClassesAsync(); 
            var model = new CreateEditStudentViewModel()
            {
                AvailableSchoolClasses = availableSchoolClasses
            };

            return View(model);
        }


        /// <summary>
        /// Handles the POST request for creating a new student.
        /// Creates a new user account for the student, assigns the "Student" role, sends a confirmation email, and saves the student details.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="model">The view model containing the new student's information.</param>
        /// <returns>Redirects to the Index action on success, or returns the view with errors.</returns>
        //POST: Students/Create
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateEditStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User //criar user antes de criar o student
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    BirthDate = model.BirthDate
                };

                var result = await _userHelper.AddUserAsync(user, "123456"); //add user depois de criado

                if (result != IdentityResult.Success) // caso não consiga criar user
                {
                    ModelState.AddModelError(string.Empty, "The user student couldn't be created");
                    return View(model); //passa modelo de volta para não ficar campos em branco
                }

                await _userHelper.AddUserToRoleAsync(user, "Student"); //adicionar role de student

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user); //gerar o token

                // gera um link de confirmção para o email
                string tokenLink = Url.Action("ResetPassword", "Account", new  //Link gerado na Action ConfirmEmail dentro do AccountController, ela recebe 2 parametros (userId e token)
                {
                    userId = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme); //utiliza o protocolo Http para passar dados de uma action para a outra

                //criar student

                //Ver se imagem foi inserida
                Guid imageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                }
                var student = new Data.Entities.Student()
                {
                    FullName = model.FullName,
                    BirthDate = model.BirthDate.Value,
                    Email = model.Email,
                    UserStudentId = user.Id,
                    SchoolClassId = model.SelectedSchoolClassId,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    ProfileImageId = imageId
                };

                await _studentRepository.CreateAsync(student);

                Response response = _mailHelper.SendEmail(student.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
               $"To allow the user,<br><br><a href = \"{tokenLink}\">Click here to confirm your email and reset password</a>"); //Contruir email e enviá-lo com o link 



                if (response.IsSuccess) //se conseguiu enviar o email
                {
                    ViewBag.Message = "Student created successfully. Confirmation instructions have been sent to the student's email";

                    return View(model);
                }

                //se não conseguiu enviar email:
                ModelState.AddModelError(string.Empty, "The student couldn't be logged");
            }
            return View(model);

        }


        /// <summary>
        /// Displays the form for editing an existing student.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="id">The ID of the student to edit.</param>
        /// <returns>A view with the student editing form, populated with the student's current information and available school classes.</returns>
        // GET: Students/Edit/5
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            //converter para model

            var model = _converterHelper.ToCreateEditStudentViewModel(student);

            if (model == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            //buscar combo de schoolclasses
            var schoolClassesList = await _schoolClassRepository.GetComboSchoolClassesAsync();

            //inserir combo no model
            model.AvailableSchoolClasses = schoolClassesList;
         

            return View(model);
        }


        /// <summary>
        /// Handles the POST request for updating an existing student.
        /// Updates the student's information, including their profile image and school class.
        /// If the school class changes, it removes any enrollments the student had in subjects no longer part of their new class's course.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="id">The ID of the student to update.</param>
        /// <param name="model">The view model containing the updated student information.</param>
        /// <returns>Redirects to the Index action on successful update, or returns the view with errors.</returns>
        // POST: Students/Edit/5
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateEditStudentViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("StudentNotFound");
            }
            

            if (ModelState.IsValid)
            {
                //buscar estudante 
                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                {
                    return new NotFoundViewResult("StudentNotFound");
                }

                //Ver se imagem foi inserida
                Guid imageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                }
                else
                { 
                    imageId = student.ProfileImageId;
                }

                //converter model para student
                student.ProfileImageId = imageId;
                student.SchoolClassId = model.SelectedSchoolClassId;
                student.Address = model.Address;
                student.BirthDate = model.BirthDate.Value;
                student.Email = model.Email;
                student.PhoneNumber = model.PhoneNumber;
                student.FullName = model.FullName;  

                //Fazer o update antes de checar os enrollments para conter a nova turma (se mudou)
                try
                {
                    await _studentRepository.UpdateAsync(student);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return new NotFoundViewResult("StudentNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }

                //checar enrollments antigos 

                var studentWithEnrollments = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(student.Id);

                var newCourseSubjectIds = studentWithEnrollments.SchoolClass.Course.CourseSubjects.Select(s => s.Id).ToList();

                foreach (var enrollment in studentWithEnrollments.Enrollments.ToList())
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

               

                return RedirectToAction(nameof(Index)); //caso user seja funcionário
            }
            model.AvailableSchoolClasses = await _schoolClassRepository.GetComboSchoolClassesAsync();
            return View(model);
        }


      

        /// <summary>
        /// Confirms and processes the deletion of a student via AJAX.
        /// </summary>
        /// <param name="id">The ID of the student to be deleted.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        // POST: Students/DeleteConfirmed/5 
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(id);

            if (student == null)
            {
                return Json(new { success = false, message = "Student not found." });
            }


            if (student.Evaluations != null && student.Evaluations.Any())
            {
                return Json(new { success = false, message = "Student cannot be deleted. There are grades associated with this student." });
            }

            //se não houver notas, desfazer relações e deletar

            if (student.Enrollments != null && student.Enrollments.Any())
            {
                foreach (var enrollment in student.Enrollments)
                {
                    await _enrollmentRepository.DeleteAsync(enrollment);    
                }
            }

            //anular a propriedade SchoolClass
            student.SchoolClass = null;

            //tornar o user do estudante inativo 
            var userStudent = await _userHelper.GetUserByEmailAsync(student.Email);

            if (userStudent != null)
            {
                userStudent.IsActive = false;
            }

            try
            {
                await _studentRepository.DeleteAsync(student);
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
        /// Retrieves the full name of a student by their ID.
        /// This method is intended to be called via AJAX.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="id">The ID of the student.</param>
        /// <returns>A JSON object containing the student's full name, or a "StudentNotFound" view if the student is not found.</returns>
        //Ajax
        [Authorize(Roles = "Employee")]
        [HttpGet("Students/GetStudentFullNameByIdAsync/{id?}")]
        public async Task<IActionResult> GetStudentFullNameByIdAsync(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id); 

            if (student == null)
            {
                return new NotFoundViewResult("StudentNotFound");
            }
   
            return Json(new { fullName = $"{student.FullName}" });
        }


        /// <summary>
        /// Checks if a student with the specified ID exists in the database.
        /// Only accessible by users with the "Employee" role.
        /// </summary>
        /// <param name="id">The ID of the student to check.</param>
        /// <returns>True if the student exists, false otherwise.</returns>
        [Authorize(Roles = "Employee")]
        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        /// <summary>
        /// Displays a generic "Student Not Found" view.
        /// </summary>
        /// <returns>The "StudentNotFound" view.</returns>
        public IActionResult StudentNotFound()
        {
            return View();
        }
    }
}
