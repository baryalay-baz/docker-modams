using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;

namespace MODAMSWeb.Config
{
    public static class MiddlewareSetup
    {
        public static void ConfigureMiddleware(WebApplication app)
        {
            // Configure middleware components in the order they should be executed
            ConfigureLocalization(app);
            ConfigureForwardedHeaders(app);
            ConfigureGlobalExceptionHandling(app);
            ConfigureSecurityMiddleware(app);
            ConfigureCors(app);
            ConfigureRoutingAndAuthentication(app);     
        }
        public static void ConfigureLocalization(WebApplication app) {
            var supportedCultures = new[] { "en", "so" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("so")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);

            localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            };
            app.UseRequestLocalization(localizationOptions);
        }
        public static void ConfigureForwardedHeaders(WebApplication app)
        {
            // Use Forwarded Headers to correctly handle requests coming through a reverse proxy
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });
        }
        public static void ConfigureGlobalExceptionHandling(WebApplication app)
        {
            // Configure global exception handling and HSTS
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
        }
        public static void ConfigureSecurityMiddleware(WebApplication app)
        {
            // Middleware for HTTPS redirection, static files, and routing
            app.UseHttpsRedirection();
            app.UseStaticFiles();
        }
        public static void ConfigureRoutingAndAuthentication(WebApplication app)
        {
            // Configure routing and authentication
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
        }
        public static void ConfigureCors(WebApplication app)
        {
            // Enable CORS policy
            app.UseCors("AllowAll");
        }

    }
}
