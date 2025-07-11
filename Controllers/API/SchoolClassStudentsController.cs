using GestaoEscolarWeb.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]

    //única maneira de Autenticar a entrada numa api é por token
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //bloqueia a API caso não tenha o token
    public class SchoolClassStudentsController : Controller
    {
        private readonly IStudentRepository _studentRepository;

        private readonly ISchoolClassRepository _schoolClassRepository;

        public SchoolClassStudentsController(IStudentRepository studentReppository, ISchoolClassRepository schoolClassRepository)
        {

            _studentRepository = studentReppository;    

            _schoolClassRepository = schoolClassRepository;
        }


        [HttpGet("{SchoolClassId}")]
        public async Task<IActionResult> GetSchoolClassStudents(int SchoolClassId)
        {  
            var schoolClass = await _schoolClassRepository.GetByIdAsync(SchoolClassId); 

            if (schoolClass == null)
            {
                return NotFound("No matching school class with this id");  
            }

            var students = await _studentRepository.GetAllStudentsWithSchoolClassAsync();
                                         
            var schoolClassStudents = students.Where(s => s.SchoolClassId == SchoolClassId).ToList();

            if (!schoolClassStudents.Any())
            {
                return NotFound("School class is empty");
            }

            return Ok(schoolClassStudents); //busca os estudantes da turma e coloca a lista dentro de um Json que retorna status OK
        }

    }
}
//explicando o flow do codigo:
//O Login é feito no AccountController, caso o user seja um Employee, ele terá acess á API. Para isso, é chamado o metodo GetJwtTokenFromServerSide() no AccountController que irá criar 
//um json do model com as informações do login que o utilizador colocar, e irá mandar uma requisição http post para a api com esse conteudo json.
//A api por sua vez, dentro do AccessApiController, no metodo Login(), irá receber [frombody] , o conteúdo json desse login e criar o token.
//Ainda dentro do metodo GetJwtTokenFromServerSide(), a resposta da requisição http post volta com o token criado para poder ser usado no acesso à api quando for realizar o GetSchoolClassStudents.
