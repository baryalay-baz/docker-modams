using MODAMSWeb.Config;

var builder = WebApplication.CreateBuilder(args);

ConfigurationHelper.ConfigureSecrets(builder);
LoggingSetup.ConfigureLogging(builder.Logging);
ServiceRegistration.AddCustomServices(builder);

var app = builder.Build();

MiddlewareSetup.ConfigureMiddleware(app);

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Users}/{controller=Home}/{action=Index}/{id?}"
);

app.Run();
