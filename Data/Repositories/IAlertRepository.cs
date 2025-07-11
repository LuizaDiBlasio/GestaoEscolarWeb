using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IAlertRepository : IGenericRepository<Alert>
    {
        Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(string userAuditId);

        Task<IEnumerable<Alert>> GetAllAlertsWithEmployee();

        List<SelectListItem> GetStatusList();


    }
}
