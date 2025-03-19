using AutoMapper;
using MediatR;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Contexts;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Internals;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace UniversalBroker.Core.Logic.Services
{
    public class ChanelJsInterpretatorService : IChanelJsInterpretatorService
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly BrockerContext _context;
        private readonly V8ScriptEngine _scriptEngine;
        private readonly JsContext _jsContext;
        private readonly AbstractDbLogingService _dbLogingService;

        public static SemaphoreSlim semaphore = new(1, 1);

        public ChanelJsInterpretatorService(
            ILogger<ChanelJsInterpretatorService> logger,
            IMapper mapper,
            BrockerContext brockerContext,
            JsContext jsContext,
            IMediator mediator,
            AbstractDbLogingService dbLogingService
            )
        {
            _logger = logger;
            _mapper = mapper;
            _context = brockerContext;
            _jsContext = jsContext;
            _mediator = mediator;
            _dbLogingService = dbLogingService;

            _scriptEngine = new V8ScriptEngine();
        }
        //TODO если скрипт - пустой, то будем просто отправлять сообщения во все каналы и подключения по цепочке, меняя только отправлителя, наверное
        public async Task ExecuteScript(Chanel chanel, InternalMessage message)
        {

            _jsContext.CurrentSenderId = chanel.Id;

            //_jsContext.Connections.ClearNoCheck();
            foreach (var connectionObj in chanel.Connections.Where(x => !x.Isinput))
            {
                _jsContext.Connections.Add(connectionObj.Name, _mapper.Map<ConnectionDto>(connectionObj));
            }

            //_jsContext.Connections.ClearNoCheck();
            foreach (var chanelObj in chanel.FromChanels.Where(x => x.Id != chanel.Id))
            {
                _jsContext.Chanels.Add(chanelObj.Name, _mapper.Map<ChanelDto>(chanelObj));
            }

            semaphore.Wait();

            try
            {
                _scriptEngine.AddHostObject("Context", _jsContext);

                _scriptEngine.AddHostObject("Message", message);

                _scriptEngine.Execute(chanel.Script.Path);
            }
            catch(Exception ex)
            {
                await _dbLogingService.LogScriptExecution(new()
                {
                    ScriptId = _jsContext.CurrentSenderId,
                    LogLevel = LogLevel.Error,
                    MessageText = ex.Message,
                });

                throw;
            }
            finally
            {
                semaphore.Release();
            }

            foreach (var item in _jsContext.GetMessages)
            {
                //TODO Победить постгрес, и можно будет распаралелить

                try
                {
                    if (item.Direction == MessageDirection.ChanelToChanel)
                        await _mediator.Send(new ExecuteScriptCommand()
                        {
                            ChanelId = item.TargetId,
                            Message = item.Message,
                        });
                    else if (item.Direction == MessageDirection.ChanelToConnection)
                        await _mediator.Send(new SendMessageCommand()
                        {
                            ConnectionId = item.TargetId,
                            Message = item.Message,
                        });
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Что-то пошло не так");
                }
                finally
                {
                    await _dbLogingService.LogMessage(item);
                }
               
            }
        }
    }
}
