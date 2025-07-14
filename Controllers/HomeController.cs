using System.Diagnostics;
using System.Threading.Tasks;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
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


        /// <summary>
        /// Displays the application's home page. If the user is authenticated and has the 'Admin' role,
        /// they will be redirected to the Dashboard. Otherwise, the default home view is displayed.
        /// </summary>
        /// <returns>An asynchronous task that returns an "IActionResult" representing the home page or a redirect.</returns>
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



        /// <summary>
        /// Displays a custom 404 Not Found error page.
        /// This action handles requests for routes that result in a 404 status.
        /// </summary>
        /// <returns>An "IActionResult" representing the 404 error view.</returns>
        [Route ("error/404")]
        public IActionResult Error404()
        {
            return View();
        }


        /// <summary>
        /// Displays the admin dashboard. This dashboard shows system alerts
        /// and system data such as passing grade and absence limit.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <returns>An asynchronous task that returns an "IActionResult" representing the dashboard view.</returns>
        //GET da view DashBoard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DashBoard()
        {
            var alerts = await _alertRepository.GetAllAlertsWithEmployee();

            var data = await _systemDataService.GetSystemDataAsync();

            var model = new DashBoardViewModel()
            {
                UserMessages = alerts,
                PassingGrade = data.PassingGrade,
                AbsenceLimitPercentage = data.AbsenceLimit * 100
            };

            return View(model);
        }


    }
}
