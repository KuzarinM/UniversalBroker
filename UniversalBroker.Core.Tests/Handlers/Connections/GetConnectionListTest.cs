using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Logic.Handlers.Queries.Connection;
using UniversalBroker.Core.Models.Queries.Connections;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Connections
{
    [TestClass]
    public class GetConnectionListTest
    {
        [TestMethod]
        public void PaginationTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionListQueryHandler>>();
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

            GetConnectionListQuery query = new GetConnectionListQuery()
            {
                PageSize = 1,
                PageNumber = 0,
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionListQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;


            // Сообщенние не null
            Assert.IsNotNull(res);

            // Данные по странице корректны
            Assert.IsTrue(
                res.PageSize == query.PageSize
                && res.CurrentPage == query.PageNumber);

            // В самом сприске всё есть
            Assert.IsTrue(
                res.Page.Count == query.PageSize);

            // В самом сприске модели те
            Assert.IsTrue(
                res.Page.All(x=>connecctions.Any(y=>y.Id == x.Id && y.Name == x.Name && y.Path == y.Path)));
        }

        [TestMethod]
        public void FiltrationTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionListQueryHandler>>();
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

            GetConnectionListQuery query = new GetConnectionListQuery()
            {
                PageSize = 10,
                PageNumber = 0,
                InputOnly = true,
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionListQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В самом сприске всё есть
            Assert.IsTrue(
                res.Page.Count == connecctions.Where(x=>x.Isinput == query.InputOnly).Count());

            // В самом сприске модели те
            Assert.IsTrue(
                res.Page.All(x => connecctions.Any(y => y.Id == x.Id && y.Name == x.Name && y.Path == y.Path)));
        }

        [TestMethod]
        public void SearchTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionListQueryHandler>>();
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
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "tes3",
                    Path = "/tes3",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connecctions);
            context.SaveChanges();

            GetConnectionListQuery query = new GetConnectionListQuery()
            {
                PageSize = 10,
                PageNumber = 0,
                NameContains = "st"
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionListQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В самом сприске всё есть
            Assert.IsTrue(
                res.Page.Count == connecctions.Where(x => x.Name.Contains(query.NameContains)).Count());

            // В самом сприске модели те
            Assert.IsTrue(
                res.Page.All(x => connecctions.Any(y => y.Id == x.Id && y.Name == x.Name && y.Path == y.Path)));
        }

        [TestMethod]
        public void EmptyTestTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetConnectionListQueryHandler>>();
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
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "tes3",
                    Path = "/tes3",
                    Isinput = true,
                    CommunicationId = communications.First().Id,
                },
            };


            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connecctions);
            context.SaveChanges();

            GetConnectionListQuery query = new GetConnectionListQuery()
            {
                PageSize = 10,
                PageNumber = 0,
                CommunicationId = Guid.NewGuid()
            };

            // Тест
            var getConnectionListCommandHandler = new GetConnectionListQueryHandler(logger.Object, mapper, context);
            var res = getConnectionListCommandHandler.Handle(query,
            default).Result;

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В самом сприске всё есть
            Assert.IsTrue(
                res.Page.Count == connecctions.Where(x => x.CommunicationId == query.CommunicationId).Count());

            // В самом сприске модели те
            Assert.IsTrue(
                res.Page.All(x => connecctions.Any(y => y.Id == x.Id && y.Name == x.Name && y.Path == y.Path)));
        }
    }
}
