using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Queries.Connection;
using UniversalBroker.Core.Models.Queries.Connections;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Connections
{
    [TestClass]
    public class GetConnectionTest
    {
        [TestMethod]
        public void PositiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionQueryHandler>>();
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
            var connecctions = new List<Connection>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test1",
                    Path = "/test1",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test2",
                    Path = "/test2",
                    Isinput = false,
                    CommunicationId = communications.First().Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test3",
                    Path = "/test3",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connecctions);
            context.SaveChanges();

            GetConnectionQuery query = new GetConnectionQuery()
            {
                Id = connecctions.First().Id
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;

            // Сообщенние не null
            Assert.IsNotNull(res);

            // Данные по Подключению корректны
            Assert.IsTrue(
                res.Id == connecctions.First().Id
                && res.IsInput == connecctions.First().Isinput
                && res.Name == connecctions.First().Name
                && res.Path == connecctions.First().Path
               );

            // Данне по Соединению корретны
            Assert.IsTrue(
                res.Communication.Name == communications.First().Name
                && res.Communication.Id == communications.First().Id
                && res.Communication.Status == communications.First().Status);
        }

        [TestMethod]
        public void NotFoundTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionQueryHandler>>();
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
            var connecctions = new List<Connection>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test1",
                    Path = "/test1",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test2",
                    Path = "/test2",
                    Isinput = false,
                    CommunicationId = communications.First().Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "test3",
                    Path = "/test3",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connecctions);
            context.SaveChanges();

            GetConnectionQuery query = new GetConnectionQuery()
            {
                Id = Guid.NewGuid()
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;

            // Сообщенние null
            Assert.IsNull(res);
        }
    }
}
