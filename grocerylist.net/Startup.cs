using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using grocerylist.net.Models.Security;
using grocerylist.net.Services;

namespace grocerylist.net
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConnector(Configuration);

            services.AddControllersWithViews();
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
            //         options => Configuration.Bind("JwtSettings", options))
            //     .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
            //         options => Configuration.Bind("CookieSettings", options));
            // services.AddAuthorization(options=>{
            //     options.AddPolicy("user", policy=>{
            //         policy.Requirements.Add(new M());
            //     });
            // });
            services.AddGroceryIdentity(Configuration);
        } // END ConfigureServices

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
        } // END Configure
    }
}
