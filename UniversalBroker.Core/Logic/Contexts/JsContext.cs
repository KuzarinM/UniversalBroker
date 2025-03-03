using MediatR;
using Microsoft.ClearScript;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Internals;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UniversalBroker.Core.Logic.Contexts
{
    /// <summary>
    /// Контекст, который передаётся внуть JS
    /// </summary>
    public class JsContext
    {
        private readonly ILogger _logger;
        private readonly IDbLogingService _dbLogingService;
        private static ConcurrentDictionary<string, string?> _internalStorage = new(); //TODO поменять на что-то более стабильное

        /// <summary>
        /// Id скрипта, где мы сейчас
        /// </summary>
        public Guid CurrentSenderId;

        /// <summary>
        /// Список каналов, которые были указаны
        /// </summary>
        public PropertyBag Chanels { get; set; } = new();

        /// <summary>
        /// Список подключений, которые были указаны
        /// </summary>
        public PropertyBag Connections { get; set; } = new();

        private ConcurrentQueue<MessageLog> _sending = new();

        /// <param name="logger"></param>
        /// <param name="dbLogingService"></param>
        public JsContext(
            ILogger<JsContext> logger,
            IDbLogingService dbLogingService)
        {
            _logger = logger;
            _dbLogingService = dbLogingService;
        }

        /// <summary>
        /// Получить все сообщения для отправки
        /// </summary>
        public List<MessageLog> GetMessages => _sending.ToList();

        #region Методы работы с каналами и подключениями

        /// <summary>
        /// Отправка сообщения в канал
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        public void SendMessageToChanel(object Id, byte[] data, ScriptObject? headers = null)
        {
            var headersDict = new Dictionary<string, string>();
            if ( headers!=null)
            {
                foreach (var name in headers.PropertyNames)
                {
                    headersDict.Add(name, headers[name].ToString()!);
                }
            }


            var message = new InternalMessage()
            {
                InternalId = Guid.NewGuid(),
                SourceId = CurrentSenderId,
                IsFromConnection = false,
                Data = data.ToList(),
                Headers = headersDict
            };

            _sending.Enqueue(new()
            {
                TargetId = Guid.Parse(Id.ToString()),
                Direction = MessageDirection.ChanelToChanel,
                Message = message,
                Created = DateTime.UtcNow,
            });
        }

        /// <summary>
        /// Отправка бинарного сообщения в канал
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        public void SendMessageToChanel(object Id, string data, ScriptObject? headers = null) =>
           SendMessageToChanel(Id, Encoding.UTF8.GetBytes(data), headers);

        /// <summary>
        /// Отправка сообщения в Подключение
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        public void SendMessageToConnection(object Id, byte[] data, ScriptObject? headers = null)
        {
            var headersDict = new Dictionary<string, string>();
            if (headers != null)
            {
                foreach (var name in headers.PropertyNames)
                {
                    headersDict.Add(name, headers[name].ToString()!);
                }
            }

            var message = new InternalMessage()
            {
                InternalId = Guid.NewGuid(),
                SourceId = CurrentSenderId,
                IsFromConnection = false,
                Data = data.ToList(),
                Headers = headersDict
            };

            _sending.Enqueue(new()
            {
                TargetId = Guid.Parse(Id.ToString()),
                Direction = MessageDirection.ChanelToConnection,
                Message = message,
                Created = DateTime.UtcNow,
            });
        }

        /// <summary>
        /// Отправка бинарного сообщения в подключение
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        public void SendMessageToConnection(object Id, string data, ScriptObject? headers = null) =>
           SendMessageToConnection(Id, Encoding.UTF8.GetBytes(data), headers);

        #endregion

        #region Методы работы с логгированием

        /// <summary>
        /// Логируем ошибки, в базу и консоль
        /// </summary>
        /// <param name="message"></param>
        public void LogError(object message)
        {
            var model = new ScriptExecutionLog()
            {
                ScriptId = CurrentSenderId,
                LogLevel = LogLevel.Error,
                MessageText = JsonConvert.SerializeObject(message)
            };

            _dbLogingService.LogScriptExecution(model);

            _logger.LogError(model.MessageText);
        }

        /// <summary>
        /// Логируем предупреждения, в базу и консоль
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(object message)
        {
            var model = new ScriptExecutionLog()
            {
                ScriptId = CurrentSenderId,
                LogLevel = LogLevel.Warning,
                MessageText = JsonConvert.SerializeObject(message)
            };

            _dbLogingService.LogScriptExecution(model);

            _logger.LogWarning(model.MessageText);
        }

        /// <summary>
        /// Логируем информацию, в базу и консоль
        /// </summary>
        /// <param name="message"></param>
        public void LogInfo(object message)
        {
            var model = new ScriptExecutionLog()
            {
                ScriptId = CurrentSenderId,
                LogLevel = LogLevel.Information,
                MessageText = JsonConvert.SerializeObject(message)
            };

            _dbLogingService.LogScriptExecution(model);

            _logger.LogInformation(model.MessageText);
        }

        #endregion

        #region Методы работы с хранилищем
        
        /// <summary>
        /// Проверить наличие значения в хранилище
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool CheckContainsInStorage(string key)
        {
            return _internalStorage.ContainsKey(key);
        }

        /// <summary>
        /// Записать в хранилище по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteIntoStorage(string key, string? value)
        {
            _internalStorage.AddOrUpdate(key, value, (k, v) => value);
        }

        /// <summary>
        /// Прочитать из хранилища по ключу
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string? ReadFromStorage(string key)
        {
            return _internalStorage.TryGetValue(key, out string res) ? res : null;
        }
        #endregion Методы работы с хранилищем
    }
}
