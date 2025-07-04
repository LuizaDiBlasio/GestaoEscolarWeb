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
            var host = CreateHostBuilder(args).Build(); // constr�i host, ambiente onde a aplica��o roda com as configura��es estabelecidas no CreateHostBuilder

            RunSeeding(host); // rodar o Seeding no host, que ir� popular o banco de dados com dados iniciais antes de a aplica��o come�ar a rodar

            host.Run(); // rodar o host (sobe o servidor web e come�a a aceitar requisi��es HTTP.)
        }

        private static void RunSeeding(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>(); // usa o IServiceScopeFactory para poder criar escopos no provedor de servi�os do host

            using (var scope = scopeFactory.CreateScope()) // cria escopo manualmente para poder atuar na DbContext fora do pipeline padr�o*
            {
                var seeder = scope.ServiceProvider.GetService<SeedDb>(); //Pega uma inst�ncia do servi�o SeedDb, respons�vel por inserir os dados iniciais na DB.

                seeder.SeedAdminAsync().Wait(); //SeedAsync e Wait garantem que o banco de dados esteja populado antes de a aplica��o come�ar a aceitar requisi��es.
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
