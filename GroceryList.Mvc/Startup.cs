using GroceryList.Mvc.Models;
using GroceryList.Mvc.Models.Config;
using GroceryList.Mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace GroceryList.Mvc
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
            //Utilities.SetUser(Configuration);
            services.AddControllersWithViews();

            // setup Database context?
            services.Configure<DataConfig>(Configuration.GetSection(DataConfig.Data));
            services.AddSingleton<IDataService, DataService>();

            services.AddTransient<IUserStore<AppUser>, UserRepository>();
            services.AddTransient<IRoleStore<AppRole>, RoleRepository>();
            services.AddTransient<IGroceryRepository, GroceryRepository>();

            services.AddIdentity<AppUser, AppRole>()
                .AddDefaultTokenProviders();
            // services.AddDefaultIdentity<AppUser>(options =>
            //     options.SignIn.RequireConfirmedAccount = true)
            //         .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    //terms
                    var googleSection = Configuration.GetSection("Authentication:Google");
                    googleOptions.ClientId = googleSection["ClientId"];
                    googleOptions.ClientSecret = googleSection["ClientSecret"];
                })
                // .AddMicrosoftAccount(microsoftOptions => { ... })
                // .AddTwitter(twitterOptions => { ... })
                // .AddFacebook(facebookOptions => { ... })
                ;
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
