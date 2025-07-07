using GestaoEscolarWeb.Data.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface IAlertRepository
    {
        Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(string userAuditId);


        Task<Alert> GetAlertByIdAsync(int id);


        Task CreateAlertAsync(Alert alert);


        Task UpdateAlertAsync(Alert alert);

    }
}
