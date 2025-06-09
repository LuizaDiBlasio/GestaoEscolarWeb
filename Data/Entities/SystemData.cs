using System.Security.Permissions;

namespace GestaoEscolarWeb.Data.Entities
{
    public class SystemData: IEntity
    {
        public int Id { get; set; }

        public decimal AbsenceLimit { get; set; } 
    }
}
