using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Communications;
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Communications
{
    [TestClass]
    public class AddOrUpdateCommunicationTest
    {
        [TestMethod]
        public void PositiveCreateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddOrUpdateCommunicationCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            Models.Dtos.Communications.CommunicationDto communicationDto = new()
            {
                Name = "test",
                Description = "test",
                TypeIdentifier = Guid.NewGuid(),
            };

            // Тест
            var updateConnectionCommandHandler = new AddOrUpdateCommunicationCommandHandler(logger.Object, mapper, context);
            updateConnectionCommandHandler.Handle(new()
            {
                CreateCommunicationDto = communicationDto
            },
            default).Wait();

            // Проверка результата

            // Мы добавили объект в БД
            var communicationInDb = context.Communications.FirstOrDefault();
            Assert.IsNotNull(communicationInDb);

            Assert.IsTrue(
                communicationInDb.Name == communicationDto.Name
                && communicationInDb.Description == communicationDto.Description
                && communicationInDb.TypeIdentifier == communicationDto.TypeIdentifier
                && communicationInDb.Status);
        }

        [TestMethod]
        public void PositiveUpdateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddOrUpdateCommunicationCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
            var communications = new List<Communication>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "test",
                    Name = "test",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            Models.Dtos.Communications.CommunicationDto communicationDto = new()
            {
                Name = communications.First().Name,
                Description = "123",
                TypeIdentifier = communications.First().TypeIdentifier,
            };

            // Тест
            var updateConnectionCommandHandler = new AddOrUpdateCommunicationCommandHandler(logger.Object, mapper, context);
            updateConnectionCommandHandler.Handle(new()
            {
                CreateCommunicationDto = communicationDto
            },
            default).Wait();

            // Проверка результата

            // Мы добавили объект в БД
            var communicationInDb = context.Communications.FirstOrDefault();
            Assert.IsNotNull(communicationInDb);

            Assert.IsTrue(
                communicationInDb.Name == communicationDto.Name
                && communicationInDb.Description == communications.First().Description
                && communicationInDb.TypeIdentifier == communicationDto.TypeIdentifier
                && communicationInDb.Status);
        }
    }
}
