using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Queries.Communication;
using UniversalBroker.Core.Models.Queries.Communications;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Communications
{
    [TestClass]
    public class GetCommunicationTest
    {
        [TestMethod]
        public void PositiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetCommunicationQueryHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var communications = new List<Communication>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test1",
                    Name = "test1",
                    TypeIdentifier = Guid.NewGuid(),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test2",
                    Name = "test2",
                    TypeIdentifier = Guid.NewGuid(),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test3",
                    Name = "test3",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            GetCommunicationQuery query = new()
            {
                Id = communications.First().Id,
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetCommunicationQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNotNull(res);

            // Проверка состава стараницы
            Assert.IsTrue(
                res.Id == communications.First().Id
                && res.Name == communications.First().Name
                && res.Status == communications.First().Status
                && res.Description == communications.First().Description
            );
        }

        [TestMethod]
        public void EmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetCommunicationQueryHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var communications = new List<Communication>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test1",
                    Name = "test1",
                    TypeIdentifier = Guid.NewGuid(),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test2",
                    Name = "test2",
                    TypeIdentifier = Guid.NewGuid(),
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test3",
                    Name = "test3",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            GetCommunicationQuery query = new()
            {
                Id = Guid.NewGuid(),
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetCommunicationQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNull(res);
        }
    }
}
