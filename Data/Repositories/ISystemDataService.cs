using GestaoEscolarWeb.Data.Entities;
using System.Threading.Tasks;

namespace GestaoEscolarWeb.Data.Repositories
{
    public interface ISystemDataService
    {
        Task<SystemData> GetSystemDataAsync();

        Task UpdateSystemDataAsync(SystemData newSystemData);
    }
}
