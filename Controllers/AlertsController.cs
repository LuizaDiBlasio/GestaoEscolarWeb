using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using GestaoEscolarWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public AlertsController(IAlertRepository alertRepository, IUserHelper userHelper, IFlashMessage flashMessage)
        {
            _alertRepository = alertRepository;

            _userHelper = userHelper;

            _flashMessage = flashMessage;
        }

        // GET: AlertsController
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

        // GET: AlertsController/Details/5
        public async  Task<IActionResult> Details(int id)
        {
            var alert = await _alertRepository.GetAlertByIdAsync(id);

            return View(alert);
        }

        // GET: AlertsController/Create
        public ActionResult Create()
        {
            return View();
        }

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
                    await _alertRepository.CreateAlertAsync(alert);

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

        // GET: AlertsController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var alert = await _alertRepository.GetAlertByIdAsync(id);   

            return View(alert);
        }

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
                    await _alertRepository.UpdateAlertAsync(alert);

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
            
    }
}

