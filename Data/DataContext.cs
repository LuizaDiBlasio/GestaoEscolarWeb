using GestaoEscolarWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace GestaoEscolarWeb.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Subject> Subjects { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<Evaluation> Evaluations { get; set; }

        public DbSet<SchoolClass> SchoolClasses { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Alert> Alerts { get; set; }

        public DbSet<User> Users { get; set; }  

        public DbSet<SystemData> SystemData { get; set; }

     

    }
}
