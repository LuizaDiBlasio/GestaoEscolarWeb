using System;

namespace GestaoEscolarWeb.Data.Entities
{
    public class Alert : IEntity
    {
        public int Id { get; set; }

        public DateTime AlertTime { get; set; }
        
        public string Message { get; set; }

        public User UserAudit { get; set; }    

        public string Status { get; set; }  //checar se vale a apena colocar

    }
}
