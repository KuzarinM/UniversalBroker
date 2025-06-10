using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Chanels;
using UniversalBroker.Core.Logic.Handlers.Queries.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Chanels
{
    [TestClass]
    public class GetChanelListTest
    {
        [TestMethod]
        public void PagingTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelListQueryHandler>>();
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "678",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = "testNew",
                    Script = "newScript"
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            GetChanelListQuery query = new()
            {
                PageNumber = 0,
                PageSize = 1
            };

            // Тест
            var addChanelCommandHandler = new GetChanelListQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.PageSize == query.PageSize
                && res.CurrentPage == query.PageNumber);

            // Даннаые корректные
            Assert.IsTrue(res.Page.All(x => chanels.Any(y => y.Id == x.Id && y.Name == y.Name)));
        }

        [TestMethod]
        public void FilterTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelListQueryHandler>>();
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "678",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = "testNew",
                    Script = "newScript"
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            GetChanelListQuery query = new()
            {
                PageNumber = 0,
                PageSize = 10,
                NameContatins = "6"
            };

            // Тест
            var addChanelCommandHandler = new GetChanelListQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.Page.Count == chanels.Where(x=>x.Name.Contains(query.NameContatins)).Count()
            );
        }

        [TestMethod]
        public void EmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetChanelListQueryHandler>>();
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "678",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = "testNew",
                    Script = "newScript"
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            GetChanelListQuery query = new()
            {
                PageNumber = 0,
                PageSize = 10,
                NameContatins = "q"
            };

            // Тест
            var addChanelCommandHandler = new GetChanelListQueryHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(query,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.Page.Count == chanels.Where(x => x.Name.Contains(query.NameContatins)).Count()
            );
        }
    }
}
