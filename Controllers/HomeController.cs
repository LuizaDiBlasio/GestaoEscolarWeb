using System.Diagnostics;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace GestaoEscolarWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IAlertRepository _alertRepository;

        private readonly IUserHelper _userHelper;

        private readonly ISystemDataService _systemDataService;

        public HomeController(ILogger<HomeController> logger, IAlertRepository alertRepository, IUserHelper userHelper,
            ISystemDataService systemDataService)
        {
            _logger = logger;

            _alertRepository = alertRepository; 

            _userHelper = userHelper;

            _systemDataService = systemDataService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user != null && await _userHelper.IsUserInRoleAsync(user, "Admin"))
                {

                    return RedirectToAction("Dashboard", "Home");
                }
            }
            return View();
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route ("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        //GET da view DashBoard
        public async Task<IActionResult> DashBoard()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);  

            if (user == null)
            {
                return View("Error");   
            }

            var alerts = await _alertRepository.GetAlertsByUserIdAsync(user.Id);

            var data = await _systemDataService.GetSystemDataAsync();

            var model = new DashBoardViewModel()
            {
                UserMessages = alerts,
                PassingGrade = data.PassingGrade,
                AbsenceLimitPercentage = data.AbsenceLimit*100
            };

            return View(model);
        }

       
    }
}
