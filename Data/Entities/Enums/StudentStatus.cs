using System;

namespace GestaoEscolarWeb.Data.Entities.Enums
{
    [Flags]
    public enum StudentStatus
    {
        //mapeameto com potencias de 2 para poder combinar bits distintos em status compostos
        Enrolled = 0,
        Approved = 1,
        Failed = 2,
        Absent = 4,
        Unknown = 16,
        Unassigned = 32
        
    }
}
