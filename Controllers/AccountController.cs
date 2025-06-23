using System;
using System.Linq;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace GestaoEscolarWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IStudentRepository _studentRepository;

        public AccountController(IUserHelper userHelper, IMailHelper mailHelper, IBlobHelper blobHelper,
            IStudentRepository studentRepository)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _studentRepository = studentRepository;

            //_countryRepository = countryRepository;

        }

        //Get do login para view onde inserir credenciais
        public IActionResult Login() //precisar criar a view do Login - clicar com o botão direito aqui em cima de Login() Add View
        {
            if (User.Identity.IsAuthenticated) //caso usuário esteja autenticado
            {
                return RedirectToAction("Index", "Home"); //mandar para a view Index que possui o controller Home
            }

            return View(); //se login não funcionar, permanece na View 
        }

        //action do login que checa as credencias na bd, faz o login de fato
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid) // se modelo enviado passar na validação
            {
                var result = await _userHelper.LoginAsync(model); //fazer login

                if (result.Succeeded) //se login for bem sucedido
                {
                    //fez login e entrou através de uma Url de retorno (quando não tem permissão para entrar numa View qualquer sem login)
                    if (this.Request.Query.Keys.Contains("ReturnUrl")) // Verifica se o URL atual (o URL da página de login) inclui um parâmetro de query chamado ReturnUrl.                                                                       
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First()); //retorna a primeira Url contendo ReturnUrl e quando faz login entra na View onde tentou entrar e não na Home)
                    }

                    return this.RedirectToAction("Index", "Home");
                }
            }

            this.ModelState.AddModelError(string.Empty, "Failed to login");

            return View(model); //model retorna pra mesma View
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register() //só mostra a view do Register
        {
            //criar modelo com as opções da combobox 

            var model = new RegisterNewUserViewModel();

            model.AvailableRoles = _userHelper.RolesToSelectList();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model) // registra o user
        {
            if (ModelState.IsValid) //ver se modelo é válido
            {
                if (model.SelectedRole == "0") // Se a opção "Select a role..." foi selecionada
                {
                    ModelState.AddModelError("SelectedRole", "You must select a valid role."); // Adiciona um erro específico para o campo SelectedRole

                    model.AvailableRoles = _userHelper.RolesToSelectList(); // repopular lista de roles

                    return View(model); // Retorna a View com o erro
                }

                var user = await _userHelper.GetUserByEmailAsync(model.Username); //buscar user  

                if (user == null) // caso user não exista, registrá-lo
                {
                    Guid imageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                    if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                    {
                        imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                    }

                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Username,
                        UserName = model.Username,
                        Address = model.Address,
                        PhoneNumber = model.PhoneNumber,
                        ImageId = imageId,
                    };

                    var result = await _userHelper.AddUserAsync(user, "123456"); //add user depois de criado

                    if (result != IdentityResult.Success) // caso não consiga criar user
                    {
                        ModelState.AddModelError(string.Empty, "The user couldn't be created");
                        return View(model); //passa modelo de volta para não ficar campos em branco
                    }

                    //Adicionar roles ao user
                    switch (model.SelectedRole)
                    {
                        case "Student":
                            await _userHelper.AddUserToRoleAsync(user, "Student");
                            break;
                        case "Employee":
                            await _userHelper.AddUserToRoleAsync(user, "Employee");
                            break;
                        case "Admin":
                            await _userHelper.AddUserToRoleAsync(user, "Admin");
                            break;
                        default:
                            ModelState.AddModelError(string.Empty, "The user couldn't be created, you need to choose a role");

                            model.AvailableRoles = _userHelper.RolesToSelectList();
                            return View(model);
                    }

                    var isStudent = await _userHelper.IsUserInRoleAsync(user, "Student");

                    if (isStudent) // caso o user seja um estudante, criar estudante
                    {
                        Student student = new Student
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.UserName,
                            Address = user.Address,
                            PhoneNumber = user.PhoneNumber,
                            UserStudentId = user.Id,
                        };

                        await _studentRepository.CreateAsync(student);
                    }

                    string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user); //gerar o token

                    // gera um link de confirmção para o email
                    string tokenLink = Url.Action("ResetPassword", "Account", new  //Link gerado na Action ConfirmEmail dentro do AccountController, ela recebe 2 parametros (userId e token)
                    {
                        userId = user.Id,
                        token = myToken
                    }, protocol: HttpContext.Request.Scheme); //utiliza o protocolo Http para passar dados de uma action para a outra

                    Response response = _mailHelper.SendEmail(model.Username, "Email confirmation", $"<h1>Email Confirmation</h1>" +
                   $"To allow the user,<br><br><a href = \"{tokenLink}\">Click here to confirm your email and reset password</a>"); //Contruir email e enviá-lo com o link 

                    if (response.IsSuccess) //se conseguiu enviar o email
                    {
                        ViewBag.Message = "Confirmation instructions have been sent to your email";

                        model.AvailableRoles = _userHelper.RolesToSelectList(); // repopular lista de roles
                        return View(model);
                    }

                    //se não conseguiu enviar email:
                    ModelState.AddModelError(string.Empty, "The user couldn't be logged");


                }
                else
                {
                    //user já existe
                    ModelState.AddModelError(string.Empty, "User already exists, try registering wih new credentials");

                    model.AvailableRoles = _userHelper.RolesToSelectList(); // repopular lista de roles
                    return View(model);
                }

            }
            model.AvailableRoles = _userHelper.RolesToSelectList(); // repopular lista de roles
            return View(model); //passa modelo de volta para não ficar campos em branco
        }

        //Get do ResetPassword
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) //verificar parâmetros
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var user = await _userHelper.GetUserByIdAsync(userId); //verificar user

            if (user == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token); //resposta do email, ver se user e token dão match

            if (!result.Succeeded)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var passwordToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

            var model = new ResetPasswordViewModel
            {
                Username = user.Email,
                Token = passwordToken
            };

            ModelState.Remove("Token"); //limpa o ModelState para evitar o uso do token antigo de confirmação de email

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) //recebo modelo preechido com dados para reset da password
        {
            var user = await _userHelper.GetUserByEmailAsync(model.Username); //buscar user

            if (user != null) //caso user exista
            {
                var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password); //fazer reset da password

                if (result.Succeeded) //se tudo correr bem
                {
                    this.ViewBag.Message = "Password reset successfully.";
                    return this.View();
                }

                //se não correr bem
                this.ViewBag.Message = "Error while resetting the password.";
                return View(model);
            }

            //caso não encontro o user
            this.ViewBag.Message = "User not found.";
            return View(model);
        }



        //GET do ChangeUser
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //buscar user por email

            var model = new ChangeUserViewModel(); //criar modelo para mostrar dados

            if (user != null) //caso user exista, preencher novo modelo com dados do user
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Address = user.Address;
                model.PhoneNumber = user.PhoneNumber;
            }

            return View(model); //retornar model novo para view
        }

        public IActionResult RecoverPassword() //direciona para view de recover da password
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model) //recebe modelo com dados recuperar password
        {
            if (this.ModelState.IsValid)//verificar se modelo é válido
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email); //buscar user
                if (user == null)
                {
                    //mensagem de erro caso user não exista
                    ModelState.AddModelError(string.Empty, "The email doesn't correspond to a registered user.");
                    return View(model);
                }

                // caso exista user prosseguir e gerar o token
                var myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user); //confirma email para depois gerar o token de reset password no GET de RestPassword

                var link = this.Url.Action( //criar o link de reset da password
                    "ResetPassword", "Account", new
                    {
                        userId = user.Id,
                        token = myToken
                    },
                    protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Email, "Shop Password Reset", $"<h1>Shop Password Reset</h1>" + //mandar o email
                    $"To reset the password click in this link </br><br/>" +
                    $"<a href = \"{link}\">Reset Password</a>");

                if (response.IsSuccess) //se correr tudo bem
                {
                    this.ViewBag.Message = "The instructions to recover your password has been sent to email.";
                }

                return this.View();
            }

            return this.View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //buscar user por email

                if (user != null) //caso user exista, user com propridades registradas no modelo
                {

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Address = model.Address;
                    user.PhoneNumber = model.PhoneNumber;

                    var response = await _userHelper.UpdateUserAsync(user); //fazer update do user

                    if (response.Succeeded)
                    {
                        ViewBag.UserMessage = "User updated";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description); //pedir a primeira mensagem de erro
                    }
                }
            }
            return View(model); //retornar model novo para view
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid) //se modelo é válido
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //verificar user
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword); //muda password

                    if (result.Succeeded)
                    {
                        return this.RedirectToAction("ChangeUser"); //redireciona para view ChangeUser
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description); //mensagem de erro
                    }
                }
                else //se for nulo
                {
                    this.ModelState.AddModelError(string.Empty, "User not found");
                }
            }

            return this.View(model); //retornar model para view caso corra mal
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        public IActionResult UserNotFound()
        {
            return View();
        }
    }
}
