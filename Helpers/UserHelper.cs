using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;    //classe que faz gestão dos utilizadores

        private readonly SignInManager<User> _signInManager; // classe que faz a gestão dos signIns

        private readonly RoleManager<IdentityRole> _roleManager; // classe que faz a gestão dos roles (usa o IdentityRole default por que não criamos a nossa para classe para os roles)

        public UserHelper(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }

        //Implementação da interface IUserHelper (são usados métodos de bypass das classes UserManage e SignInManager)


        /// <summary>
        /// Creates a new user with the specified password.
        /// </summary>
        /// <param name="user">The "User" entity to create.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the operation.
        /// </returns>
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }


        /// <summary>
        /// Asynchronously adds a user to a specified role.
        /// </summary>
        /// <param name="user">The "User" entity to add to the role.</param>
        /// <param name="roleName">The name of the role to add the user to.</param>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }


        /// <summary>
        /// Asynchronously changes the password for a user.
        /// </summary>
        /// <param name="user">The "User" entity whose password will be changed.</param>
        /// <param name="oldPassword">The current password of the user.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>
        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }


        /// <summary>
        /// Asynchronously checks if a role exists, if it doesn't it creates the role.
        /// </summary>
        /// <param name="roleName">The name of the role to check.</param>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName); // se existe, buscar o role

            if (!roleExists) // se não existe, criar
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName
                });
            }

        }


        /// <summary>
        /// Creates predefined roles ("Employee", "Student", "Admin") if they do not already exist in the system.
        /// </summary>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task CreateRolesAsync()
        {
            string[] roleNames = { "Employee", "Student", "Admin" }; 

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);

                if (!roleExists)// Verifica se a role já existe
                {     
                    await _roleManager.CreateAsync(new IdentityRole(roleName));  // Se não existir, cria a role
                }
            }
        }


        /// <summary>
        /// Creates a list of "SelectListItem" representing predefined user roles for a dropdown in the user interface.
        /// </summary>
        /// <returns>
        /// A "List{T}" of "SelectListItem" containing options for "Select a role...", "Employee", "Student", and "Admin".
        /// </returns>
        public List<SelectListItem> RolesToSelectList()
        {   
            var selectList = new List<SelectListItem> //converter para SelectListItem
            {
                new SelectListItem{Value = "0", Text = "Select a role..."},
                new SelectListItem{Value = "Employee", Text = "Employee"},
                new SelectListItem{Value = "Student", Text = "Student"},
                new SelectListItem{Value = "Admin", Text = "Admin"},
            };
            return selectList;
        }


        /// <summary>
        /// Asynchronously retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>
        /// A "Task{TResult} that represents the asynchronous operation.
        /// The task result contains the "User" entity if found, otherwise "null".
        /// </returns>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        /// <summary>
        /// Asynchronously checks if a user is a member of a specified role.
        /// </summary>
        /// <param name="user">The "User" entity to check.</param>
        /// <param name="roleName">The name of the role to check against.</param>
        /// <returns>
        /// A "Task{TResult} that represents the asynchronous operation.
        /// The task result contains "true" if the user is in the specified role, otherwise "false".
        /// </returns>
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName); //devolve uma booleana dizendo se user está no role ou não
        }


        /// <summary>
        /// Asynchronously attempts to sign in a user with the provided credentials.
        /// </summary>
        /// <param name="model">The "LoginViewModel" containing the username, password, and remember me preference.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains a "SignInResult" indicating the outcome of the sign-in attempt.
        /// </returns>
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }


        /// <summary>
        /// Asynchronously signs out the currently authenticated user.
        /// </summary>
        /// <returns>A "Task" that represents the asynchronous operation.</returns>
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }


        /// <summary>
        /// Asynchronously updates the user's information in the data store.
        /// </summary>
        /// <param name="user">The "User" entity with updated properties.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the update.
        /// </returns>
        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }


        /// <summary>
        /// Asynchronously generates an email confirmation token for the specified user.
        /// </summary>
        /// <param name="user">The "User" entity for whom to generate the token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the generated email confirmation token.
        /// </returns>
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously generates a password reset token for the specified user.
        /// </summary>
        /// <param name="user">The "User" entity for whom to generate the token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the generated password reset token.
        /// </returns>
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }


        /// <summary>
        /// Asynchronously confirms the user's email address using a token.
        /// </summary>
        /// <param name="user">The "User" entity whose email is to be confirmed.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the confirmation.
        /// </returns>
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }


        /// <summary>
        /// Asynchronously retrieves a user by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains the "User" entity if found, otherwise "null".
        /// </returns>
        public Task<User> GetUserByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }


        /// <summary>
        /// Asynchronously resets a user's password using a password reset token.
        /// </summary>
        /// <param name="user">The "User" entity whose password is to be reset.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="password">The new password for the user.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains an "IdentityResult" indicating the success or failure of the password reset.
        /// </returns>
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }


        /// <summary>
        /// Asynchronously validates a user's password without signing them in.
        /// </summary>
        /// <param name="user">The "User" entity whose password is to be validated.</param>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation.
        /// The task result contains a "SignInResult" indicating whether the password is valid.
        /// </returns>
        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false); //o false é para não bloquear após muitas tentativas, em ambiente de produção, tem que ser true
        }
    }
}
