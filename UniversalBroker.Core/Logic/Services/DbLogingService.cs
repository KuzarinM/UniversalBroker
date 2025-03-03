using AutoMapper;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Services
{
    public class DbLogingService : IDbLogingService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        private readonly ConcurrentQueue<MessageLog> messageLogs = new();
        private readonly ConcurrentQueue<ScriptExecutionLog> scriptExecutionLogs = new();

        public DbLogingService(
            ILogger<DbLogingService> logger,
            IMapper mapper,
            Func<BrockerContext> context
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = context();
        }

        public Task LogMessage(MessageLog log)
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

        public Task LogScriptExecution(ScriptExecutionLog log)
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

        public Task StartLogging(CancellationToken cancellationToken)
        {
            _ = Task.Run(() => StoreLogs(cancellationToken));

            return Task.CompletedTask;
        }

        private async Task StoreLogs(CancellationToken token)
        {
            int stopper = 100;

            while (!token.IsCancellationRequested) 
            {
                if(messageLogs.Count > 0 || scriptExecutionLogs.Count > 0)
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
    }
}
