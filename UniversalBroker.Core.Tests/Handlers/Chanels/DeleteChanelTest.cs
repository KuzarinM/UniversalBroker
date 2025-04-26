using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Chanels
{
    [TestClass]
    public class DeleteChanelTest
    {
        [TestMethod]
        public void PositiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<DeleteChanelCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123"
                }
            };
            // вставка тестовых данных
            context.Chanels.AddRange(chanels);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new DeleteChanelCommandHandler(logger.Object, mapper, context);
            addChanelCommandHandler.Handle(new()
            {
                Id = chanels.First().Id
            },
            default).Wait();

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault();
            Assert.IsNull(record);
        }

        [TestMethod]
        public void NotExistsTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<DeleteChanelCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123"
                }
            };
            // вставка тестовых данных
            context.Chanels.AddRange(chanels);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new DeleteChanelCommandHandler(logger.Object, mapper, context);
            addChanelCommandHandler.Handle(new()
            {
                Id = Guid.NewGuid()
            },
            default).Wait();

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault();
            Assert.IsNotNull(record);
        }
    }
}
