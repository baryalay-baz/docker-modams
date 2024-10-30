using MODAMSWeb.Config;

var builder = WebApplication.CreateBuilder(args);

ConfigurationHelper.ConfigureSecrets(builder);
LoggingSetup.ConfigureLogging(builder.Logging);
ServiceRegistration.AddCustomServices(builder);

var app = builder.Build();

MiddlewareSetup.ConfigureForwardedHeaders(app);

// Error Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Users/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Users}/{controller=Home}/{action=Index}/{id?}"
);

app.Run();
