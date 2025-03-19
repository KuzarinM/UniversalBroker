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

        public override async Task LogMessage(MessageLog log)
        {
            try
            {
                messageLogs.Enqueue(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при внесении сообщения в лог");
            }
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
            try
            {
                var message = _mapper.Map<Message>(messageLog);

                await _context.Messages.AddAsync(message);

                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Не удалось сохранить сообщение");

                // Если кто-то попытался отправить что-то не то и не в туда, то мы залогируем. Прицидент был
                if (messageLog.Direction != Models.Enums.MessageDirection.ConnectionToChanel)
                    await LogScriptExecution(new()
                    {
                        ScriptId = messageLog.Message.SourceId,
                        LogLevel = LogLevel.Error,
                        MessageText = ex.Message,
                    });
            }

        }

        private async Task SaveExecutionToDb(ScriptExecutionLog executionLog)
        {
            try
            {
                var log = _mapper.Map<ExecutionLog>(executionLog);

                await _context.ExecutionLogs.AddAsync(log);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось сохранить лог");
            }

        }

        private async Task StartWorking(CancellationToken stoppingToken)
        {
            int stopper = 100;

            while (!stoppingToken.IsCancellationRequested)
            {
                if (messageLogs.Count > 0 || scriptExecutionLogs.Count > 0)
                {
                    stopper = 100;

                    try
                    {
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
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Ошибка при итерации по очереди");
                    }
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(()=>StartWorking(stoppingToken));
        }
    }
}
