using GestaoEscolarWeb.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq;

namespace GestaoEscolarWeb.Data
{
    public class DataContext : IdentityDbContext<User>
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

        protected override void OnModelCreating(ModelBuilder modelBuilder) //desabilita deletar em cascata direto na base de dados para impedir múltiplos caminhos de deleção em cascata.
        {
            base.OnModelCreating(modelBuilder); // configura tabelas padrão do Identity

            //antes de criar o modelo
            var cascadeFKs = modelBuilder.Model
                 .GetEntityTypes() //buscar todas as entidades
                 .SelectMany(t => t.GetForeignKeys()) //selecionar todas as chaves estrangeiras 
                 .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade); // que tenham comportamento em cascata (relações com outras tabelas)

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict; // restringe comportamento ao deletar, se houver entidades filhas, não deleta. Deve ser deletado por código individualmente.
            }
        }
    }

}
