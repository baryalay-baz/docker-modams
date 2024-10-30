namespace MODAMSWeb.Config
{
    public static class ConfigurationHelper
    {
        public static void ConfigureSecrets(WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;
            configuration.AddEnvironmentVariables();
        }
    }
}
