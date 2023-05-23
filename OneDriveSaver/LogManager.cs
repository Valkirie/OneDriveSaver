using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Steeltoe.Extensions.Configuration.Placeholder;
using System;
using System.Configuration;
using System.Diagnostics;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace OneDriveSaver
{
    public static class LogManager
    {
        private static ILogger logger;
        public static void Initialize(string name)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"{name}.json")
                .AddPlaceholderResolver()
                .Build();

            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.File($".\\logs\\OneDriveSaver{Environment.MachineName}_.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            logger = new SerilogLoggerFactory(serilogLogger).CreateLogger(name);
        }

        public static void LogInformation(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
            logger.LogInformation(message, args);
        }

        public static void LogWarning(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
            logger.LogWarning(message, args);
        }

        public static void LogCritical(string message, params object[] args)
        {
            Trace.TraceError(message, args);
            logger.LogCritical(message, args);

            // crash app
            throw new Exception(message);
        }

        public static void LogDebug(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
            logger.LogDebug(message, args);
        }

        public static void LogError(string message, params object[] args)
        {
            Trace.TraceError(message, args);
            logger.LogError(message, args);
        }
    }
}
