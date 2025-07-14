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


        /// <summary>
        /// Retrieves the single system data record from the database.
        /// There will always be exactly one record (with Id = 1).
        /// If the record is not found (e.g., if seeding failed), it returns a new, default <see cref="SystemData"/> instance.
        /// </summary>
        /// <returns>
        /// A "Task{TResult}" that represents the asynchronous operation containing the "SystemData" entity if found,
        /// otherwise a new "SystemData" instance with default values.
        /// </returns>
        public async Task<SystemData> GetSystemDataAsync()
        { 
            var systemData = await _context.SystemData.FirstOrDefaultAsync(sd => sd.Id == 1); //sempre um registo

            //caso o seed falhe
            return systemData ?? new SystemData();
        }


        /// <summary>
        /// Updates the existing system data record in the database.
        /// It expects to find an existing record based on the "newSystemData" entity's Id.
        /// </summary>
        /// <param name="newSystemData">The "SystemData" entity containing the updated values (AbsenceLimit and PassingGrade).</param>
        /// <returns>A "Task" that represents the asynchronous update operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the existing SystemData entry with the given Id is not found for update.</exception>
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
