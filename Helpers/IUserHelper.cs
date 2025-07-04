﻿using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserByEmailAsync(string email); //passa o email para buscar user

        Task<IdentityResult> AddUserAsync(User user, string password); //adiciona user na BD

        Task<SignInResult> LoginAsync(LoginViewModel model); //método que devolve tarefa SignInResult (ou tá signed in ou não)

        Task LogoutAsync();

        Task<IdentityResult> UpdateUserAsync(User user); //update user na BD

        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword); //muda pass do user 

        Task CheckRoleAsync(string roleName); //verifica se existem roles

        Task AddUserToRoleAsync(User user, string roleName); // designa o role ao user

        Task<bool> IsUserInRoleAsync(User user, string roleName); // verifica se user está designado ao role

        Task CreateRolesAsync(); //cria roles

        List<SelectListItem> RolesToSelectList(); //coloca roles em uma lista

        //TODO fazer quando for fazer API
        /*Task<SignInResult> ValidatePasswordAsync(User user, string password);*/ //não faz login, só valida a password para acesso à API

        Task<string> GenerateEmailConfirmationTokenAsync(User user); //Gera o email de confirmação e insere o Token 

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token); //Valida o email, verifica se o token é valido

        Task<User> GetUserByIdAsync(string id); //recebe um id e devolve o user correspondente

        Task<IdentityResult> ResetPasswordAsync(User user, string token, string password); //Faz o reset da password
    }
}
