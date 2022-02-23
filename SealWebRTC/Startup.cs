using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SealWebRTC.Data;
using SealWebRTC.Hubs;
using SealWebRTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SealWebRTC
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
            services.AddSession(delegate (SessionOptions s)
            {
                s.IdleTimeout = TimeSpan.FromMinutes(480.0);
            });
            services.AddSession();

            

            var connection = Configuration.GetConnectionString("DB");
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 24));
            services.AddDbContext<EFContext>(options => options.UseMySql(connection, serverVersion));

#if (DEBUG)
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(AuthenticationFilter));
            }).AddRazorRuntimeCompilation();
#else
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(AuthenticationFilter));
            });
#endif

            //services.AddScoped<IDatabaseChangeNotificationService, SqlDependecyService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(provider => provider.GetService<IHttpContextAccessor>()?.HttpContext?.User);
            services.AddSignalR();

            //services.AddSingleton<OpenTokService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var path = System.IO.Directory.GetCurrentDirectory();
            //loggerFactory.AddFile($"{path}\\Logs\\Log-{Date}.txt");
            loggerFactory.AddFile("Logs/Log-{Date}.txt");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseExceptionHandler("/Autenticacion/PaginaNoEncontrada");
            app.UseStatusCodePagesWithReExecute("/Autenticacion/PaginaNoEncontrada", "?statusCode={0}");

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller}/{action}/{id?}");
                endpoints.MapHub<TicketHub>("/ticketHub");
                endpoints.MapHub<DashHub>("/dashHub");
            });
        }
    }
}
