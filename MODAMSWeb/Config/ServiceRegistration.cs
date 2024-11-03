using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MODAMS.ApplicationServices;
using MODAMS.DataAccess.Data;
using MODAMS.Utility;
using Telerik.Reporting.Services;
using Telerik.Reporting.Cache.File;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MODAMS.ApplicationServices.IServices;

namespace MODAMSWeb.Config
{
    public class ServiceRegistration
    {
        public static void AddCustomServices(WebApplicationBuilder builder)
        {
            // Add Kendo UI
            builder.Services.AddKendo();

            // Add Identity services and configure Identity options
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity options
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            // Configure application cookies
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/AccessDenied");
                options.Cookie.Name = "AMSCookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/Login");
                options.LogoutPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/Logout");
                options.ReturnUrlParameter = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            // Add DbContext and configure connection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // Add CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader();
                });
            });

            // Configure IIS options (if needed)
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // Add Razor Pages and Controllers with Views
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews().AddNewtonsoftJson();

            // Add Reporting services configuration
            builder.Services.TryAddScoped<IReportServiceConfiguration>(sp => new ReportServiceConfiguration
            {
                ReportingEngineConfiguration = sp.GetService<IConfiguration>(),
                HostAppId = "MODAMS",
                Storage = new FileStorage(),
                ReportSourceResolver = sp.GetRequiredService<IReportSourceResolver>()
            });

            builder.Services.AddScoped<IReportSourceResolver, CustomReportSourceResolver>();

            // Register custom services (specific to MODAMS)
            builder.Services.AddSingleton<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IAMSFunc, AMSFunc>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IAlertService, AlertService>();
            builder.Services.AddScoped<IAssetService, AssetService>();
            builder.Services.AddScoped<IDisposalService, DisposalService>();
            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<ITransferService, TransferService>();
            builder.Services.AddScoped<ICategoriesService, CategoriesService>();
            builder.Services.AddScoped<IDonorService, DonorService>();
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IEmployeesService, EmployeesService>();
            builder.Services.AddScoped<IDepartmentsService, DepartmentsService>();
            builder.Services.AddScoped<IVerificationService, VerificationService>();

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();
        }
    }
}
