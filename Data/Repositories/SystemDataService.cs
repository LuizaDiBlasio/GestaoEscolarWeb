using GestaoEscolarWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public class SystemDataService : ISystemDataService
    {
        private readonly DataContext _context;

        public SystemDataService(DataContext context) 
        {
            _context = context;
        }

        public async Task<SystemData> GetSystemDataAsync()
        { 
            var systemData = await _context.SystemData.FirstOrDefaultAsync(sd => sd.Id == 1); //sempre um registo

            //caso o seed falhe
            return systemData ?? new SystemData();
        }

        public async Task UpdateSystemDataAsync(SystemData newSystemData)
        {
            var existingData = await _context.SystemData.FirstOrDefaultAsync(sd => sd.Id == newSystemData.Id);

            if (existingData != null)
            {
                existingData.AbsenceLimit = newSystemData.AbsenceLimit;
                existingData.PassingGrade = newSystemData.PassingGrade;

                _context.SystemData.Update(existingData); 
                await _context.SaveChangesAsync();

            }
            else
            {
                throw new InvalidOperationException("SystemData entry not found for update.");
            }
        }
    }
}
