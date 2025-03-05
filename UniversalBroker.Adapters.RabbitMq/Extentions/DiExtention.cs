using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.Reflection;
using LogLevel = NLog.LogLevel;

namespace UniversalBroker.Adapters.RabbitMq.Extentions
{
    public static class DiExtention
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddSingletons();

            services.AddScopeds();

            services.AddGrpc();

            return services;
        }

        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddScopeds(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddGrpc(this IServiceCollection services)
        {
            return services;
        }

        public static void AddLogger(this WebApplicationBuilder builder)
        {
            var nLogConfig = new LoggingConfiguration();
            var logConsole = new ConsoleTarget();
            var blackhole = new NullTarget();

            var logFile = new FileTarget()
            {
                FileName = "${basedir}/logs/${shortdate}_logs.log"
            };

            nLogConfig.AddRule(LogLevel.Trace, LogLevel.Trace, blackhole, "Microsoft.AspNetCore.*", true);
            nLogConfig.AddRule(LogLevel.Info, LogLevel.Warn, logFile, "Microsoft.EntityFrameworkCore.*", true);
            nLogConfig.AddRule(LogLevel.Info, LogLevel.Warn, logFile, "Microsoft.AspNetCore.*", true);
            nLogConfig.AddRule(LogLevel.Info, LogLevel.Warn, logFile, "System.Net.Http.HttpClient.Refit.*", true);
            nLogConfig.AddRule(LogLevel.Debug, LogLevel.Debug, logConsole, "Goldev.Core.*");
            nLogConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            nLogConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            builder.Logging.ClearProviders();
            builder.Services.AddLogging(m => m.AddNLog(nLogConfig));
        }
    }
}
