using AutoMapper;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Threading;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Services
{
    public class DbLogingService(
        ILogger<DbLogingService> logger,
        IMapper mapper,
        Func<BrockerContext> context
        ) : AbstractDbLogingService
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = context();
        private readonly ConcurrentQueue<MessageLog> messageLogs = new();
        private readonly ConcurrentQueue<ScriptExecutionLog> scriptExecutionLogs = new();

        public override Task LogMessage(MessageLog log)
        {
            try
            {
                messageLogs.Enqueue(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при внесении сообщения в лог");
            }

            return Task.CompletedTask;
        }

        public override Task LogScriptExecution(ScriptExecutionLog log)
        {
            try
            {
                scriptExecutionLogs.Enqueue(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при внесении информации из скрипта в лог");
            }

            return Task.CompletedTask;
        }

        private async Task SaveMessageToDb(MessageLog messageLog)
        {
            var message = _mapper.Map<Message>(messageLog);

            await _context.Messages.AddAsync(message);
        }

        private async Task SaveExecutionToDb(ScriptExecutionLog executionLog)
        {
            var log = _mapper.Map<ExecutionLog>(executionLog);

            await _context.ExecutionLogs.AddAsync(log);
        }

        private async Task StartWorking(CancellationToken stoppingToken)
        {
            int stopper = 100;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (messageLogs.Count > 0 || scriptExecutionLogs.Count > 0)
                {
                    stopper = 100;

                    while (messageLogs.Count > 0 || scriptExecutionLogs.Count > 0)
                    {
                        if (messageLogs.TryDequeue(out MessageLog messageLog))
                            await SaveMessageToDb(messageLog);
                        if (scriptExecutionLogs.TryDequeue(out ScriptExecutionLog scriptLog))
                            await SaveExecutionToDb(scriptLog);

                        stopper--;
                        if (stopper < 0)
                            break;
                    }

                    await _context.SaveChangesAsync();
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(()=>StartWorking(stoppingToken));
        }
    }
}
