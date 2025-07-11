using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using System;
using System.Threading.Tasks;
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


        public AlertsController(IAlertRepository alertRepository, IUserHelper userHelper, IFlashMessage flashMessage, ISystemDataService systemDataService )
        {
            _alertRepository = alertRepository;

            _userHelper = userHelper;

            _flashMessage = flashMessage;

            _systemDataService = systemDataService;
        }

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


        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Details/5
        public async  Task<IActionResult> Details(int id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);

            return View(alert);
        }


        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Create
        public ActionResult Create()
        {
            return View();
        }


        [Authorize(Roles = "Employee")]
        // POST: AlertsController/Create
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


        [Authorize(Roles = "Employee")]
        // GET: AlertsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);   

            return View(alert);
        }



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

        [Authorize(Roles = "Admin")]
        //Post de ManageTicket
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


        public IActionResult AlertNotFound()
        {
            return View();
        }

    }
}

