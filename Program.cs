using GestaoEscolarWeb.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestaoEscolarWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build(); // constrói host, ambiente onde a aplicação roda com as configurações estabelecidas no CreateHostBuilder

            RunSeeding(host); // rodar o Seeding no host, que irá popular o banco de dados com dados iniciais antes de a aplicação começar a rodar

            host.Run(); // rodar o host (sobe o servidor web e começa a aceitar requisições HTTP.)
        }

        private static void RunSeeding(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>(); // usa o IServiceScopeFactory para poder criar escopos no provedor de serviços do host

            using (var scope = scopeFactory.CreateScope()) // cria escopo manualmente para poder atuar na DbContext fora do pipeline padrão*
            {
                var seeder = scope.ServiceProvider.GetService<SeedDb>(); //Pega uma instância do serviço SeedDb, responsável por inserir os dados iniciais na DB.

                seeder.SeedAdminAsync().Wait(); //SeedAsync e Wait garantem que o banco de dados esteja populado antes de a aplicação começar a aceitar requisições.
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
