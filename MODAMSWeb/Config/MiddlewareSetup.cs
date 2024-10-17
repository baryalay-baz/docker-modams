using Microsoft.AspNetCore.HttpOverrides;

namespace MODAMSWeb.Config
{
    public static class MiddlewareSetup
    {
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
