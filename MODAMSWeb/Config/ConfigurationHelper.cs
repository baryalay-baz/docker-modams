namespace MODAMSWeb.Config
{
    public static class ConfigurationHelper
    {
        public static void ConfigureSecrets(WebApplicationBuilder builder)
        {
            // Load and bind configuration secrets (like those from Azure Key Vault, AWS Secrets Manager, or environment variables)
            var configuration = builder.Configuration;

            // Example: Loading from environment variables or a secrets file (custom implementation)
            configuration.AddEnvironmentVariables();

            // If you're using a secrets file, you can load it like this:
            // configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

            // Example of binding configuration to a strongly-typed class (if needed)
            // var mySettings = new MySettings();
            // configuration.GetSection("MySettingsSection").Bind(mySettings);

            // Inject settings into services (if using strongly-typed settings)
            // builder.Services.Configure<MySettings>(configuration.GetSection("MySettingsSection"));
        }
    }
}
