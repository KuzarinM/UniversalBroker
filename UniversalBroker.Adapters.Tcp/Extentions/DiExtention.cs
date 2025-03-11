using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.Reflection;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Logic.Services;
using static Protos.CoreService;
using LogLevel = NLog.LogLevel;

namespace UniversalBroker.Adapters.Tcp.Extentions
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

            services.AddHostedService(p => p.GetRequiredService<IInitService>());

            return services;
        }

        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            services.AddSingleton<IInitService, InitService>();
            services.AddSingleton<TcpManager>();
            return services;
        }

        public static IServiceCollection AddScopeds(this IServiceCollection services)
        {
            services.AddScoped<IMainService, MainService>();
            services.AddScoped<TcpClientService>();
            return services;
        }

        public static IServiceCollection AddGrpc(this IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IOptions<BaseConfiguration>>().Value;

                var channel = GrpcChannel.ForAddress(config.CoreBaseUrl);

                return new CoreServiceClient(channel);
            });

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
