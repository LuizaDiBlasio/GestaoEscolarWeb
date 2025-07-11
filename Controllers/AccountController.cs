using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Vereyon.Web;
using Microsoft.AspNetCore.Http;


namespace GestaoEscolarWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMailHelper _mailHelper;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IStudentRepository _studentRepository;
        private readonly IConverterHelper _converterHelper;
        private readonly IFlashMessage _flashMessage;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AccountController(IUserHelper userHelper, IMailHelper mailHelper, IBlobHelper blobHelper,
            IStudentRepository studentRepository, IFlashMessage flashMessage, IConverterHelper converterHelper, 
            IConfiguration configuration, HttpClient httpClient)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _blobHelper = blobHelper;
            _studentRepository = studentRepository;
            _converterHelper = converterHelper;
            _flashMessage = flashMessage;
            _configuration = configuration;
            _httpClient = httpClient;   

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
                var result = await _userHelper.LoginAsync(model); //fazer login (Este é o seu método que usa PasswordSignInAsync internamente)

                if (result.Succeeded) //se login for bem sucedido
                {
                    var user = await _userHelper.GetUserByEmailAsync(model.Username);

                    // Verificação de usuário e roles para a API e redirecionamento (existente) 
                    if (user == null)
                    {
                        _flashMessage.Danger("User not found");
                        return View(model);
                    }

                    var isEmployee = await _userHelper.IsUserInRoleAsync(user, "Employee");
                    var isAdmin= await _userHelper.IsUserInRoleAsync(user, "Admin");

                    if (isAdmin) // Verifica novamente se é Admin para redirecionar para o Dashboard
                    {
                        return this.RedirectToAction("DashBoard", "Home");
                    }

                    if (isEmployee) // Só tenta obter o token se for Employee
                    {
                        var token = await GetJwtTokenFromServerSide(model); 

                        if (!string.IsNullOrEmpty(token))
                        {
                            // Armazenar o token para uso posterior 
                            HttpContext.Session.SetString("JwtToken", token);

                            // Redirecionamento 
                            if (this.Request.Query.Keys.Contains("ReturnUrl"))
                            {
                                return Redirect(this.Request.Query["ReturnUrl"].First());
                            }

                            

                            return this.RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            await _userHelper.LogoutAsync(); // Desloga 
                            _flashMessage.Danger("Login failed, token generation error");
                            return View(model);
                        }
                    }
                    else // Se não for Admin nem Employee, mas o login  foi bem-sucedido
                    {

                        if (this.Request.Query.Keys.Contains("ReturnUrl"))
                        {
                            return Redirect(this.Request.Query["ReturnUrl"].First());
                        }

                        return this.RedirectToAction("Index", "Home");
                    }
                }
            }
            // Se o result.Succeeded for false (login falhou )
            this.ModelState.AddModelError(string.Empty, "Failed to login");
            _flashMessage.Danger("Login failed. Invalid credentials.");

            return View(model); 
        }


        private async Task<string> GetJwtTokenFromServerSide(LoginViewModel model)
        {
            try
            { 
                string apiUrl = "https://localhost:44385/api/AccessApi/Login"; //Url de login na api

                // Serializa o model do login para JSON
                var jsonContent = JsonSerializer.Serialize(model);

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); //empacota a string JSON e informa ao servidor que o corpo da requisição é JSON

                var response = await _httpClient.PostAsync(apiUrl, content); //envia o 'content' (que contém o JSON do model) para o 'apiUrl'.
                                                                             //método 'Login' na API espera receber 'content' como '[FromBody] LoginViewModel model'

                if (response.IsSuccessStatusCode) //se o envio correu bem
                {
                    //ler e Desserializar a resposta
                    var responseContent = await response.Content.ReadAsStringAsync(); 
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponseModel>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return tokenResponse?.Token; // Retorna o valor da propriedade 'Token' do objeto 'tokenResponse', e se a resposta for null, retornar null
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(); //mostrar erro na resposta do json
                    Console.WriteLine($"Error occured while trying to get API token: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured : {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Register() //só mostra a view do Register
        {
            //criar modelo com as opções da combobox 

            var model = new RegisterNewUserViewModel();

            model.AvailableRoles = _userHelper.RolesToSelectList();

            return View(model);
        }


        [Authorize(Roles = "Admin")]
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

                var user = await _userHelper.GetUserByEmailAsync(model.Email); //buscar user  

                if (user == null) // caso user não exista, registrá-lo
                {
                    Guid imageId = Guid.Empty; // identificador da imagem no blob (ainda não identificada)

                    if (model.ImageFile != null && model.ImageFile.Length > 0) //verificar se existe a imagem
                    {
                        imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens"); //manda gravar o ficheiros na pasta imagens 
                    }

                    user = new User
                    {
                        FullName = model.FullName,
                        Email = model.Email,
                        UserName = model.Email,
                        Address = model.Address,
                        PhoneNumber = model.PhoneNumber,
                        ImageId = imageId,
                        BirthDate = model.BirthDate.Value
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
                            FullName = user.FullName,
                            Email = user.UserName,
                            Address = user.Address,
                            PhoneNumber = user.PhoneNumber,
                            UserStudentId = user.Id,
                            BirthDate = user.BirthDate.Value
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

                    Response response = _mailHelper.SendEmail(model.Email, "Email confirmation", $"<h1>Email Confirmation</h1>" +
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
                model.FullName = user.FullName;
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

                    user.FullName = model.FullName;
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


        [Authorize(Roles = "Admin, Employee")]
        //GET
        public async Task<IActionResult> MyUserProfile()
        {
            var username = this.User.Identity.Name; //pegar o username

            var user = await _userHelper.GetUserByEmailAsync(username);

            if (user == null)
            {
                _flashMessage.Danger("User not found");
                return View();
            }

            //converter user para model
            var model = _converterHelper.ToMyUserProfileViewModel(user);

            //mandar model para a view
            return View(model);
        }


        [Authorize(Roles = "Admin, Employee")]
        [HttpPost]
        public async Task<IActionResult> MyUserProfile(MyUserProfileViewModel model)
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

            if (user == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ver se já tem imagem
                    Guid currentImageId = user.ImageId ?? Guid.Empty;

                    // confirmar se carregou image file
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        currentImageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "imagens");
                    }
                    else
                    {
                        //se não houver imagem
                        if (!model.ImageId.HasValue || model.ImageId.Value == Guid.Empty)
                        {
                            currentImageId = Guid.Empty; // zerar guid por garntia
                        }
                    }

                    user.FullName = model.FullName;
                    user.BirthDate = model.BirthDate;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;

                    // Atribuir nova guid ou guid antiga
                    user.ImageId = currentImageId != Guid.Empty ? currentImageId : (Guid?)null;


                    var result = await _userHelper.UpdateUserAsync(user);

                    if (!result.Succeeded)
                    {
                        _flashMessage.Danger("Error updating user profile.");
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }

                    _flashMessage.Confirmation($"Your profile has been updated successfully.");


                    return RedirectToAction(nameof(MyUserProfile));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ViewBag.ErrorMessage = "Cannot save data. Unexpected Error";
                    return View(model);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"An unexpected error occurred: {ex.Message}";

                    return View(model);
                }
            }

            return View(model);

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
