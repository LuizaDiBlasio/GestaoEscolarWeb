using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;

        private readonly IUserHelper _userHelper;


        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAdminAsync() //método para inicializar com admin
        {
            await _context.Database.MigrateAsync(); // Faz migrações pendentes. Caso não exista BD, cria uma BD aos moldes do _context 

            await _userHelper.CreateRolesAsync();

            await _userHelper.CheckRoleAsync("Admin"); //verificar se já existe um role de admin, se não existir cria

            var user = await _userHelper.GetUserByEmailAsync("Luizabandeira90@gmail.com"); //ver se user já existe 

            if (user == null) // caso não encontre o utilizador 
            {
                user = new User // cria utilizador admin
                {
                    FullName = "Luiza Bandeira",
                    Email = "luizabandeira90@gmail.com",
                    UserName = "luizabandeira90@gmail.com",
                    PhoneNumber = "12345678",
                    Address = "Fonte da Saudade"
                };

                var result = await _userHelper.AddUserAsync(user, "123456"); //criar utilizador, mandar utilizador e password

                if (result != IdentityResult.Success) //se o resultado não for bem sucedido (usa propriedade da classe Identity) 
                {
                    throw new InvalidOperationException("Coud not create the user in seeder"); //pára o programa
                }

                await _userHelper.AddUserToRoleAsync(user, "Admin"); //adiciona role ao user
            }

            var isInRole = await _userHelper.IsUserInRoleAsync(user, "Admin"); //verifica se role foi designado para user existente

            if (!isInRole) //se não estiver no role, colocar
            {
                await _userHelper.AddUserToRoleAsync(user, "Admin"); //adiciona role ao user
            }
        }
    }
}
