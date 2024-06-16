using GroceryList.Models.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace GroceryList
{
    // Test Home ID: c254f50f-10b5-4f89-af7a-bab17fe78c45
    // https://localhost:44380/c254f50f-10b5-4f89-af7a-bab17fe78c45/grocery
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment currentEnv;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            currentEnv = hostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            // var safeIpAddress = System.Net.IPAddress.Parse(Configuration.GetValue<string>("ForwardOptions:SafeIpAddress"));
            // services.Configure<ForwardedHeadersOptions>(o => o.KnownProxies.Add(safeIpAddress));

            services.Configure<GeneralConfig>(Configuration.GetSection("General"));
            services.Configure<DataServiceConfig>(Configuration.GetSection("DataService"));
            services.AddSingleton<Data.IUpdateCache, Data.UpdateCache>();

            // depending on configuration (file OR database)
            if (Configuration.GetValue<bool>("UseMariaDB")) //useMariaDb)
            {
                services.AddSingleton<Lib.IResourceMapper, Services.ResourceMapper>();
                services.AddSingleton<System.Data.Common.DbProviderFactory>(MySqlConnector.MySqlConnectorFactory.Instance);
                services.AddScoped<Services.IDataService, Data.DbDataService>();
                //var userStoreType = typeof(DbUserRepository<,>).MakeGenericType(builder.UserType, typeof(TDocumentStore));
                //builder.Services.AddScoped(typeof(IUserStore<>).MakeGenericType(builder.UserType), userStoreType);
                //services.AddScoped<Microsoft.AspNetCore.Identity.IUserStore<>, Data.DbUserRepository>();
                //services.AddScoped<Data.DbUserRepository>();
                //services.AddScoped<Data.DbRoleRepository>();
                services.AddScoped<Lib.IGroceryRepository, Db.DbGroceryRepository>();
            }
            else
            {
                services.AddScoped<Services.IDataService, Services.FileDataService>();
                //services.AddScoped<IUserDataRepository, UserDataRepository>();
                //services.AddScoped<Data.UserRepository>();
                //services.AddScoped<Data.RoleRepository>();
                services.AddScoped<Lib.IGroceryRepository, Data.FileGroceryRepository>();
            }

            services.AddScoped<Services.HomeRouteFilter>(); // should this be Transient?

            //services.AddDbContext<ApplicationDbContext>(o=> o.UseMySql())
            //services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //services.AddDatabaseDeveloperPageExceptionFilter();
            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSignalR();

            //.AddMvc vs .AddControllersWithViews
            var mvc = services.AddControllersWithViews(o => o.Filters.AddService<Services.HomeRouteFilter>());

            if (currentEnv.IsDevelopment()) mvc.AddRazorRuntimeCompilation();
        } // END ConfigureServices

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseMigrationsEndPoint();
                // Production will use Proxy
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseRouting();
            app.UseStatusCodePages();

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllers();
                //endpoints.MapHttpAttributeRoutes();

                //endpoints.MapDefaultControllerRoute();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<Services.GroceryHub>("/grocery-hub");
            });
        } // END Configure
    }
}
