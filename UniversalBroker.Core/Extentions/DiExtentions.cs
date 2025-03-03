using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LogLevel = NLog.LogLevel;
using NLog.Extensions.Logging;
using NLog.Targets;
using System.Reflection;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Services;
using NLog.Config;
using UniversalBroker.Core.Logic.Contexts;
using System;
using UniversalBroker.Core.Logic.Abstracts;

namespace UniversalBroker.Core.Extentions
{
    public static class DiExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection services) 
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddSingletons();

            services.AddScopeds();

            services.AddGrpc();

            services.AddHostedService(p => p.GetRequiredService<AbstractDbLogingService>());

            services.AddSwaggerStaf();

            services.AddDatabase();
            return services;
        }

        public static IServiceCollection AddSingletons(this IServiceCollection services)
        {
            services.AddSingleton<AbstractDbLogingService, DbLogingService>();
            services.AddSingleton<Func<BrockerContext>>(sp => () => sp.CreateAsyncScope().ServiceProvider.GetService<BrockerContext>()!);
            return services;
        }

        public static IServiceCollection AddScopeds(this IServiceCollection services)
        {
            services.AddTransient<IChanelJsInterpretatorService, ChanelJsInterpretatorService>();
            services.AddTransient<JsContext>();

            return services;
        }

        public static IServiceCollection AddGrpc(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddSwaggerStaf(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddGrpcSwagger();
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddDbContextPool<BrockerContext>(cfg =>
            {
                cfg.UseNpgsql("Password=postgres;Username=postgres;Database=brocker;Host=192.168.254.121");
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
