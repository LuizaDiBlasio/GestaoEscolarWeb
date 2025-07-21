using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class AlertRepository : GenericRepository<Alert>, IAlertRepository
    {
        private readonly DataContext _context;

        public AlertRepository(DataContext context) : base(context) 
        {
            _context = context;
        }



        /// <summary>
        /// Retrieves a collection of alerts associated with a specific user.
        /// </summary>
        /// <param name="userAuditId">The unique identifier of the user (UserAuditId).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// an "IEnumerable{T}" of type "Alert" entities belonging to the specified user.</returns>
        public async Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(string userAuditId)
        {
            return await _context.Alerts
                    .Where(a => a.UserAuditId == userAuditId)
                    .ToListAsync();

        }

        /// <summary>
        /// Retrieves an alert with its User
        /// </summary>
        /// <param name="id">The unique identifier of the alert</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// an object of type "Alert" with its user loaded</returns>
        public async Task<Alert> GetAlertWithUserAsync(int id)
        {
            return await _context.Alerts
                .Include(a => a.UserAudit)
                .FirstOrDefaultAsync(a => a.Id == id);
        }


        /// <summary>
        /// Retrieves a collection of all alerts, including their associated employee (UserAudit) information.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// an "IEnumerable{T}" of "Alert" entities, with the UserAudit navigation property loaded.</returns>
        public async Task<IEnumerable<Alert>> GetAllAlertsWithEmployee()
        {
            return await _context.Alerts.Include(a => a.UserAudit).ToListAsync();
        }


        /// <summary>
        /// Gets a list of "SelectListItem" representing the possible statuses from the "Status" enumeration.
        /// This is typically used for populating dropdown lists in UI.
        /// </summary>
        /// <returns>A "List{T}" of "SelectListItem" where each item's Value and Text are the string representation of a "Status" enum member.</returns>
        public List<SelectListItem> GetStatusList()
        {
            return Enum.GetValues(typeof(Status)) //indicar o tipo do enum
                      .Cast<Status>() // converter ints do enum para lista IEnumerable<Status>
                      .Select(status => new SelectListItem //converter para lista SelectListItem 
                      {
                          Value = status.ToString(), 
                          Text = status.ToString()   
                      }).ToList();
        }

    }
}
