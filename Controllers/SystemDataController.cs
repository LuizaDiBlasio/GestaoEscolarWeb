using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vereyon.Web;

namespace GestaoEscolarWeb.Controllers
{
    public class SystemDataController : Controller
    {
        private readonly ISystemDataService _systemDataService;

        private readonly IFlashMessage _flashMessage;

        private readonly IUserHelper _userHelper;

        private readonly IAlertRepository _alertRepository;

        public SystemDataController(ISystemDataService systemData, IFlashMessage flashMessage,
            IUserHelper userHelper, IAlertRepository alertRepository)
        {
            _systemDataService = systemData;

            _flashMessage = flashMessage;

            _alertRepository = alertRepository;

            _userHelper = userHelper;
        }


        /// <summary>
        /// Handles the POST request for updating system data settings.
        /// This method receives updated system data from the Dashboard view.
        /// It validates the input, updates the system data, and provides feedback to the user via flash messages.
        /// </summary>
        /// <param name="model">A "DashBoardViewModel" containing the updated system data settings.</param>
        /// <returns>
        /// If the model state is invalid, it reloads an error message. On successful update, it redirects to the Dashboard.
        /// In case of an unexpected error during the update, it displays an error message and reloads the Dashboard view.
        /// </returns>
        // POST: SystemDataController/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(DashBoardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _flashMessage.Danger("Please insert a valid number");

                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);

                if (user == null)
                {
                    return View("Error");
                }

                var alerts = await _alertRepository.GetAlertsByUserIdAsync(user.Id);

                model.UserMessages = alerts;

                return View("~/Views/Home/Dashboard.cshtml", model);
            }


            var data = await _systemDataService.GetSystemDataAsync();


            data.AbsenceLimit = model.AbsenceLimitPercentage / 100; // Tirar a percentagem
            data.PassingGrade = model.PassingGrade;

            try
            {
                await _systemDataService.UpdateSystemDataAsync(data);
                _flashMessage.Confirmation("Data submitted successfully!");
                return RedirectToAction("Dashboard", "Home");
            }
            catch (System.Exception ex)
            {
                _flashMessage.Danger($"An unexpected error occurred while submitting data: {ex.Message}");

                return View("~/Views/Home/Dashboard.cshtml", model);
            }

        }


    }
}
