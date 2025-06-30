using System;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Migrations;
using GestaoEscolarWeb.Models;
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

       

        public StudentsController(DataContext context, IUserHelper userHelper, IBlobHelper blobHelper,
            IStudentRepository studentRepository, IFlashMessage flashMessage, IConverterHelper converterHelper,
            IMailHelper mailHelper, ISchoolClassRepository schoolClassRepository)
        {
            _context = context;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _studentRepository = studentRepository;
            _flashMessage = flashMessage;   
            _schoolClassRepository = schoolClassRepository; 
            _converterHelper = converterHelper; 
            _mailHelper = mailHelper;   
              
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllStudentsWithSchoolClassAsync();
            return View(students);
        }

        //Get do MyProfile
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

        // GET: Students/Details/5
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

        // GET: Students/Create
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


        //POST: Students/Create
       [HttpPost]
        public async Task<IActionResult> Create(CreateEditStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User //user vai ser sempre null antes de criar o student
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    UserName = model.Email,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber
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
            

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
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

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateEditStudentViewModel model)
        {
            if (id != model.Id)
            {
                return new NotFoundViewResult("StudentNotFound");
            }

            if (ModelState.IsValid)
            {
                //Ver se imagem foi inserida
                Guid imageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                }

                //converter model para student
                var student = _converterHelper.FromCreateEditToStudent(model, false, imageId);

                if (student == null)
                {
                    return new NotFoundViewResult("StudentNotFound");
                }

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

                return RedirectToAction(nameof(Index)); //caso user seja funcionário
            }
            return View(model);
        }

        // POST: Students/MyProfile/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                    await _studentRepository.UpdateAsync(student);
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


        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _studentRepository.GetStudentWithSchoolClassEnrollmentsAndEvaluationsAsync(id);

            if (student.Evaluations != null && student.Evaluations.Any())
            {
                _flashMessage.Danger("Student cannot be deleted. There are grades associated with this student.");
                return RedirectToAction(nameof(Delete), new { id = student.Id });
            }

            //se não houver notas, desfazer relações e deletar

            if(student.Enrollments != null && student.Enrollments.Any())
            {
                student.Enrollments.Clear();
            }

            //anular a propriedade SchoolClass
            student.SchoolClass = null;

            try
            {
                await _studentRepository.DeleteAsync(student);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorTitle = $"Failed to delete student '{student.FullName}'.";

                string errorMessage = "An unexpected database error occurred.";

                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }

                ViewBag.ErrorMessage = errorMessage;

                return View("Error");
            }
            
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        public IActionResult StudentNotFound()
        {
            return View();
        }
    }
}
