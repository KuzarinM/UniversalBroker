using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Queries.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Chanels
{
    [TestClass]
    public class GetChanelScriptLogsTest
    {
        [TestMethod]
        public void PaginationTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelScriptLogsQueryHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var communications = new List<Communication>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = true,
                    Description = "test",
                    Name = "test",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                }
            };
            var executionLog = new List<ExecutionLog>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "INFO",
                    ScriptId = scripts.First().Id,
                    Text = "log1"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "DEBUG",
                    ScriptId = scripts.First().Id,
                    Text = "log2"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "INFO",
                    ScriptId = scripts.First().Id,
                    Text = "log3"
                },
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.ExecutionLogs.AddRange(executionLog);
            context.SaveChanges();

            GetChanelScriptLogsQuery query = new()
            {
                PageSize = 1,
                PageNumber = 0,
                ChanelId = chanels.First().Id,
            };

            // Тест
            var addChanelCommandHandler = new GetChanelScriptLogsQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.PageSize == query.PageSize
                && res.CurrentPage == query.PageNumber
            );

            Assert.IsTrue(
                res.Page.All(x=>executionLog.Any(y=>x.DateTime == y.Datetime && x.Text == y.Text)));
        }

        [TestMethod]
        public void EmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelScriptLogsQueryHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var communications = new List<Communication>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = true,
                    Description = "test",
                    Name = "test",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                }
            };
            var executionLog = new List<ExecutionLog>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "INFO",
                    ScriptId = scripts.First().Id,
                    Text = "log1"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "DEBUG",
                    ScriptId = scripts.First().Id,
                    Text = "log2"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Lavel = "INFO",
                    ScriptId = scripts.First().Id,
                    Text = "log3"
                },
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.ExecutionLogs.AddRange(executionLog);
            context.SaveChanges();

            GetChanelScriptLogsQuery query = new()
            {
                PageSize = 100,
                PageNumber = 0,
                ChanelId = Guid.NewGuid(),
            };

            // Тест
            var addChanelCommandHandler = new GetChanelScriptLogsQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(res.Page.Count() == 0);
        }
    }
}
