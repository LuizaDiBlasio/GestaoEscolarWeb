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
       


        public async Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(string userAuditId)
        {
            return await _context.Alerts
                    .Where(a => a.UserAuditId == userAuditId)
                    .ToListAsync();

        }

        public async Task<IEnumerable<Alert>> GetAllAlertsWithEmployee()
        {
            return await _context.Alerts.Include(a => a.UserAudit).ToListAsync();
        }

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
