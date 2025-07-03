using System.ComponentModel;

namespace GestaoEscolarWeb.Data.Entities.Enums
{
    public enum StudentStatus
    {
        Enrolled = 0,
        Approved = 1,
        Failed = 2,

        [Description("Failed (Excessive Absences)")]
        FailedByAbsence = 3,

        [Description("Status Not Found")]
        NotFound = 4,

        [Description("Not Enrolled")]
        NotEnrolled = 5
    }
}
