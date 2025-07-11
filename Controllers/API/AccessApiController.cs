using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.EJ2.Grids;

namespace GestaoEscolarWeb.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessApiController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;

        public AccessApiController(IUserHelper userHelper, IConfiguration configuration)
        {
            _userHelper = userHelper;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Login")] // Rota: /api/AccessApi/Login
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    // Verifica se o utilizador tem a role de Employee 
                    var isEmployee = await _userHelper.IsUserInRoleAsync(user, "Employee");
              

                    if (isEmployee) // se for o employee, criar token de acesso à api
                    {
                        var result = await _userHelper.ValidatePasswordAsync(user, model.Password); //validar password

                        if (result.Succeeded)
                        {
                            var claims = new[] //criar as claims que o token irá conter (infos de email, guid do token e role)
                            {
                                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
                                new Claim(ClaimTypes.Role,  "Employee" )
                            };

                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"])); //criar a chave do token
                            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // criar as credenciais com a chave
                            
                            //crira token com as configurações do app settings, claims, credenciais e exp date
                            var token = new JwtSecurityToken(
                                _configuration["Tokens:Issuer"],
                                _configuration["Tokens:Audience"],
                                claims,
                                expires: DateTime.UtcNow.AddDays(15),
                                signingCredentials: credentials);

                            var results = new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(token), //serializar o token em uma string
                                expiration = token.ValidTo // passar a data no formato DateTime
                            };

                            return Created(string.Empty, results); //indica a criação da autenticação por token e manda o token serializado
                        }
                    }
                }
            }
            return BadRequest("Invalid user credentials"); 
        }
    }
}
