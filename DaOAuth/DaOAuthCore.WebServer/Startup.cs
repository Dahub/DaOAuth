using DaOAuthCore.Dal.EF;
using DaOAuthCore.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DaOAuthCore.WebServer
{
    public class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfiguration>(Configuration.GetSection("AppConfiguration"));

            services.AddAuthentication("DaOAuth")
                   .AddCookie("DaOAuth", options =>
                   {
                       options.AccessDeniedPath = new PathString("/Home/Index");
                       options.LoginPath = new PathString("/Home/Index");
                   });

            services.AddSingleton<IClientService>(new ClientService()
            {
                Configuration = Configuration.GetSection("AppConfiguration").Get<AppConfiguration>(),
                Factory = new EfRepositoriesFactory()
            });
            services.AddSingleton<IUserClientService>(new UserClientService()
            {
                Configuration = Configuration.GetSection("AppConfiguration").Get<AppConfiguration>(),
                Factory = new EfRepositoriesFactory()
            });
            services.AddSingleton<IUserService>(new UserService()
            {
                Configuration = Configuration.GetSection("AppConfiguration").Get<AppConfiguration>(),
                Factory = new EfRepositoriesFactory()
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
