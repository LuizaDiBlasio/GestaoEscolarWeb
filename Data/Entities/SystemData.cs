namespace GestaoEscolarWeb.Data.Entities
{
    public class SystemData : IEntity
    {
        public int Id { get; set; }

        public decimal AbsenceLimit { get; set; }

        public decimal PassingGrade { get; set; }

        public SystemData() // contrutor com valores standard
        {
            Id = 1; 
            AbsenceLimit = 0.25m; 
            PassingGrade = 10.0m; 
        }
    }
}
