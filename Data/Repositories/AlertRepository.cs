using GestaoEscolarWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Syncfusion.EJ2.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly DataContext _context;

        public AlertRepository(DataContext context) 
        {
            _context = context;
        }
        public async Task CreateAlertAsync(Alert alert)
        {
            await _context.Alerts.AddAsync(alert); //adicionar à tabela 

            await SaveAllAsync(); //salvar
        }

        public async Task<Alert> GetAlertByIdAsync(int id)
        {
            return await _context.Alerts
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(string userAuditId)
        {
            return await _context.Alerts
                    .Where(a => a.UserAuditId == userAuditId)
                    .ToListAsync();

        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            _context.Alerts.Update(alert); 

            await SaveAllAsync();
        }

        private async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
