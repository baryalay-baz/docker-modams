namespace MODAMSWeb.Config
{
    public static class LoggingSetup
    {
        public static void ConfigureLogging(ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            logging.AddEventSourceLogger();
            logging.SetMinimumLevel(LogLevel.Debug);
        }
    }
}
