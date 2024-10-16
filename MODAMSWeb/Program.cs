using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MODAMS.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MODAMS.Utility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Telerik.Reporting.Services;
using Telerik.Reporting.Cache.File;
using System.Runtime.InteropServices;
using MODAMS.ApplicationServices;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add EventLog only on Windows
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Logging.AddEventLog(new EventLogSettings
    {
        SourceName = "MODAMS"
    });
}

// Configure services for ReportsController
builder.Services.TryAddScoped<IReportServiceConfiguration>(sp => new ReportServiceConfiguration
{
    ReportingEngineConfiguration = sp.GetService<IConfiguration>(),
    HostAppId = "MODAMS",
    Storage = new FileStorage(),
    ReportSourceResolver = sp.GetRequiredService<IReportSourceResolver>()
});

builder.Services.AddScoped<IReportSourceResolver, CustomReportSourceResolver>();

// Add Kendo UI
builder.Services.AddKendo();

// Add Identity services and configure Identity options
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/AccessDenied");
    options.Cookie.Name = "AMSCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/Login");
    options.LogoutPath = new Microsoft.AspNetCore.Http.PathString("/Identity/Account/Logout");
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

// Add DbContext and configure CORS
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
    });
});

// Configure IIS options
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// Add Razor Pages and other required services
builder.Services.AddRazorPages();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//Add custom services
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



builder.Services.AddHttpContextAccessor();

// Add Controllers with Views and configure JSON serialization
builder.Services.AddControllersWithViews().AddNewtonsoftJson();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowAll");

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Users}/{controller=Home}/{action=Index}/{id?}"
);

app.Run();

static string GetReportsDir(IServiceProvider sp)
{
    return Path.Combine(sp.GetService<IWebHostEnvironment>().ContentRootPath, "Reports");
}
