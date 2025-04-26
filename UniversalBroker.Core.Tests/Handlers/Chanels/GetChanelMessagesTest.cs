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
    public class GetChanelMessagesTest
    {
        [TestMethod]
        public void PaginationTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelMessagesQueryHandler>>();
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "456",
                    Script = scripts.First()
                }
            };
            var messages = new List<Message>()
            {
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    TargetChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>x.Isinput).First().Id,
                },
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    SourceChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>!x.Isinput).First().Id,
                },
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    TargetChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>x.Isinput).First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            GetChanelMessagesQuery query = new()
            {
                PageSize = 1,
                PageNumber = 0,
                ChanelId = chanels.First().Id,
            };

            // Тест
            var addChanelCommandHandler = new GetChanelMessagesQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.PageSize == query.PageSize
                && res.CurrentPage == query.PageNumber
            );
        }

        [TestMethod]
        public void EmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelMessagesQueryHandler>>();
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "456",
                    Script = scripts.First()
                }
            };
            var messages = new List<Message>()
            {
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    TargetChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>x.Isinput).First().Id,
                },
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    SourceChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>!x.Isinput).First().Id,
                },
                new()
                {
                    Id= Guid.NewGuid(),
                    Datetime = DateTime.Now,
                    Data = [],
                    TargetChannelId = chanels.First().Id,
                    ConnectionId = connections.Where(x=>x.Isinput).First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            GetChanelMessagesQuery query = new()
            {
                PageSize = 1,
                PageNumber = 0,
                ChanelId = Guid.NewGuid(),
            };

            // Тест
            var addChanelCommandHandler = new GetChanelMessagesQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(res.Page.Count()==0);
        }
    }
}
