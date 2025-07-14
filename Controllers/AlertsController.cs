using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using System;
using System.Threading.Tasks;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace GestaoEscolarWeb.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class AlertsController : Controller
    {
        private readonly IAlertRepository _alertRepository;

        private readonly IUserHelper _userHelper;

        private readonly IFlashMessage _flashMessage;

        private readonly ISystemDataService _systemDataService;


        public AlertsController(IAlertRepository alertRepository, IUserHelper userHelper, IFlashMessage flashMessage, ISystemDataService systemDataService)
        {
            _alertRepository = alertRepository;

            _userHelper = userHelper;

            _flashMessage = flashMessage;

            _systemDataService = systemDataService;
        }

        /// <summary>
        /// Displays a list of alerts specific to the currently logged-in user.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>A view displaying the user's messages.</returns>
        // GET: AlertsController
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
            //buscar o user

            var username = this.User.Identity.Name;

            var user = await _userHelper.GetUserByEmailAsync(username);

            //buscar as mensagens do user

            var messages = await _alertRepository.GetAlertsByUserIdAsync(user.Id);

            //criar model com mensagens
            //
            var model = new UserMessagesViewModel()
            {
                UserMessages = messages
            };

            return View(model);
        }


        /// <summary>
        /// Displays the details of a specific alert.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the alert to display.</param>
        /// <returns>A view displaying the alert details.</returns>
        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);

            return View(alert);
        }


        /// <summary>
        /// Displays the view for creating a new alert.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <returns>The alert creation view.</returns>
        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Create
        public ActionResult Create()
        {
            return View();
        }


        /// <summary>
        /// Processes the creation of a new alert.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="alert">The alert object containing the new message details.</param>
        /// <returns>Redirects to the Index view on success, or returns the view with an error message on failure.</returns>
        // POST: AlertsController/Create
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> Create(Alert alert)
        {
            if (ModelState.IsValid)
            {
                //buscar o user

                var username = this.User.Identity.Name;

                var user = await _userHelper.GetUserByEmailAsync(username);

                //preecher outras propriedades programaticamente
                alert.AlertTime = DateTime.Now;
                alert.Status = Data.Entities.Enums.Status.Unresolved;
                alert.UserAuditId = user.Id;

                try
                {
                    await _alertRepository.CreateAsync(alert);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger("An unexpected error occured while sending this message");
                    return View(alert);
                }
            }
            return View(alert);

        }

        /// <summary>
        /// Displays the view for editing an existing alert.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the alert to edit.</param>
        /// <returns>A view displaying the alert to be edited.</returns>
        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);

            return View(alert);
        }


        /// <summary>
        /// Processes the update of an existing alert.
        /// This action is accessible only by users with the 'Employee' role.
        /// </summary>
        /// <param name="id">The ID of the alert being edited.</param>
        /// <param name="alert">The alert object containing the updated message details.</param>
        /// <returns>Redirects to the Index view on success, or returns the view with an error message on failure.</returns>
        [Authorize(Roles = "Employee")]
        // POST: AlertsController/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Alert alert)
        {
            if (ModelState.IsValid)
            {
                //buscar o user

                var username = this.User.Identity.Name;

                var user = await _userHelper.GetUserByEmailAsync(username);


                //preecher outras propriedades programaticamente
                alert.AlertTime = DateTime.Now;
                alert.Status = Data.Entities.Enums.Status.Unresolved;
                alert.UserAuditId = user.Id;

                try
                {
                    await _alertRepository.UpdateAsync(alert);

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    _flashMessage.Danger("An unexpected error occured while editing this message");
                    return View(alert);
                }
            }

            return View(alert);
        }


        /// <summary>
        /// Displays the view for managing a specific alert (ticket).
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the alert (ticket) to manage.</param>
        /// <returns>A view displaying the alert details for management.</returns>
        [Authorize(Roles = "Admin")]
        //Get ManageTicket
        public async Task<IActionResult> ManageTicket(int id)
        {
            var statusList = _alertRepository.GetStatusList();

            var alert = await _alertRepository.GetByIdAsync(id);

            var model = new ManageTicketViewModel()
            {
                Id = alert.Id,
                StatusList = statusList,
                UserFullName = alert.UserAudit.FullName,
                AlertTime = alert.AlertTime,
                SelectedStatus = alert.Status,
                MessageTitle = alert.MessageTitle,
                Message = alert.Message

            };
            return View(model);
        }


        /// <summary>
        /// Processes the management and update of an alert (ticket) status.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="model">The view model containing the updated alert status and details.</param>
        /// <param name="id">The ID of the alert (ticket) being managed.</param>
        /// <returns>Redirects to the Dashboard on success, or returns the view with an error message on failure.</returns>
        [Authorize(Roles = "Admin")]
        //Post de ManageTicket
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ManageTicket(ManageTicketViewModel model, int id)
        {
            if (ModelState.IsValid)
            {
                var alert = await _alertRepository.GetByIdAsync(id);

                alert.Status = model.SelectedStatus;

                try
                {
                    await _alertRepository.UpdateAsync(alert);

                    var alerts = _alertRepository.GetAll();

                    var data = await _systemDataService.GetSystemDataAsync();

                    var modelDashBoard = new DashBoardViewModel()
                    {
                        UserMessages = alerts,
                        PassingGrade = data.PassingGrade,
                        AbsenceLimitPercentage = data.AbsenceLimit * 100
                    };

                    return View("~/Views/Home/DashBoard.cshtml", modelDashBoard);
                }
                catch
                {
                    _flashMessage.Danger("An unexpected error occured while managing this ticket");
                    return View(model);
                }
            }
            return View(model);
        }


        /// <summary>
        /// Deletes a specific alert.
        /// This action is accessible only by users with the 'Admin' role.
        /// </summary>
        /// <param name="id">The ID of the alert to delete.</param>
        /// <returns>Redirects to the Dashboard view with a success or error message.</returns>
        // GET: Subjects/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            if (id == 0)
            {
                _flashMessage.Danger("Invalid alert ID provided for deletion.");
                return RedirectToAction("DashBoard", "Home");
            }

            var alert = await _alertRepository.GetByIdAsync(id);

            if (alert == null)
            {
                _flashMessage.Warning("Alert not found for deletion.");
                return RedirectToAction("DashBoard", "Home");
            }

            try
            {
                await _alertRepository.DeleteAsync(alert); // Assumindo que DeleteAsync é o método de exclusão
                _flashMessage.Confirmation("Alert deleted successfully!");
            }
            catch (Exception ex)
            {
                string errorMessage = "An unexpected database error occurred.";
                if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }
                _flashMessage.Danger($"{errorMessage}");

            }

            var alerts = _alertRepository.GetAll();

            var data = await _systemDataService.GetSystemDataAsync();

            var modelDashBoard = new DashBoardViewModel()
            {
                UserMessages = alerts,
                PassingGrade = data.PassingGrade,
                AbsenceLimitPercentage = data.AbsenceLimit * 100
            };

            return View("~/Views/Home/DashBoard.cshtml", modelDashBoard);
        }


        /// <summary>
        /// Displays the "Alert Not Found" view when a requested alert is not found.
        /// </summary>
        /// <returns>The "Alert Not Found" view.</returns>
        public IActionResult AlertNotFound()
        {
            return View();
        }

    }
}

