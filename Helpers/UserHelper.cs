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
        private readonly UserManager<User> _userManager;    //clase que faz gestão dos utilizadores

        private readonly SignInManager<User> _signInManager; // classe que faz a gestão dos signIns

        private readonly RoleManager<IdentityRole> _roleManager; // classe que faz a gestão dos roles (usa o IdentityRole default por que não criamos a nossa para classe para os roles)

        public UserHelper(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }

        //Implementação da interface IUserHelper (são usados métodos de bypass das classes UserManage e SignInManager, com exceção do método CheckRoleAsync)
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        public async Task CheckRoleAsync(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName); // se existe, buscar o role

        }

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

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName); //devolve uma booleana dizendo se user está no role ou não
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public Task<User> GetUserByIdAsync(string id)
        {
            return _userManager.FindByIdAsync(id);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }
    }
}
