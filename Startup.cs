using GestaoEscolarWeb.Data;
using GestaoEscolarWeb.Data.Entities;
using GestaoEscolarWeb.Data.Repositories;
using GestaoEscolarWeb.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncfusion.Licensing;
using Vereyon.Web;


namespace GestaoEscolarWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration; 
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2XFhhQlJHfVhdX2NWfFN0QHNRdV5wfldPcC0sT3RfQFhjT3xXd0ZmX3xdcnxdQmteWA ==");

            services.AddIdentity<User, IdentityRole>(cfg => //adicionar servi�o de Identiy para ter o user e configurar o servi�o
            {
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireLowercase = false;
                cfg.Password.RequireUppercase = false;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequiredLength = 6;

            }).AddEntityFrameworkStores<DataContext>() //Depois do servi�o implementado continua a usar o DataContext, aplicar o servi�o criado � BD
              .AddDefaultTokenProviders();

            //TODO quando criar a API
            //adicionar servico de autentifica��o do Token e configurar os par�metros
            //vai ser utilizado na autentica��o do user quando for usar a API dos produtos
            //services.AddAuthentication().AddCookie().AddJwtBearer(cfg =>
            //{
            //    //mandar configura��es do token
            //    cfg.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidIssuer = this.Configuration["Tokens:Issuer"],
            //        ValidAudience = this.Configuration["Tokens:Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Tokens:Key"]))
            //    };
            //});

            services.AddDbContext<DataContext>(cfg =>
            {
                cfg.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddFlashMessage();

            services.AddTransient<SeedDb>();

            services.AddScoped<IUserHelper, UserHelper>();

            services.AddScoped<IStudentRepository, StudentRepository>();    

            services.AddScoped<ISubjectRepository, SubjectRepository>();

            services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();

            services.AddScoped<ICourseRepository, CourseRepository>();

            services.AddScoped<IConverterHelper, ConverterHelper>();

            services.AddScoped<IBlobHelper, BlobHelper>();

            services.AddScoped<IMailHelper, MailHelper>();

            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

            services.AddScoped<IEvaluationRepository, EvaluationRepository>();

            services.AddScoped<ISystemDataService, SystemDataService>();

            services.AddScoped<IAlertRepository, AlertRepository>();


            //anula o ReturnUrl no Login (AccountController) e nega acesso n�o autorizado
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/NotAuthorized";
                options.AccessDeniedPath = "/Account/NotAuthorized";
            });

            services.AddControllersWithViews();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
