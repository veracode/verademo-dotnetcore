using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Verademo.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Verademo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSession(options => {
                options.Cookie.SameSite = Environment.GetEnvironmentVariable("APPLICATION_URL") == null ? SameSiteMode.Lax : SameSiteMode.None;
                options.Cookie.SecurePolicy = Environment.GetEnvironmentVariable("APPLICATION_URL") == null ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            });

            // services.AddAntiforgery(options => 
            // {
            //     options.Cookie.SameSite = Environment.GetEnvironmentVariable("APPLICATION_URL") == null ? SameSiteMode.Lax : SameSiteMode.None;
            //     options.Cookie.SecurePolicy = Environment.GetEnvironmentVariable("APPLICATION_URL") == null ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            // });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}");
                endpoints.MapRazorPages();
            });
        }
    }
}