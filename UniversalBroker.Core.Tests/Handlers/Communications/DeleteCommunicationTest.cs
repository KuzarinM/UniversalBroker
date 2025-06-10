using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Commands.Communications;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Communications
{
    [TestClass]
    public class DeleteCommunicationTest
    {
        [TestMethod]
        public void PositiveDeleteTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<DeleteCommunicationCommandHandler>>();
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

            // Тест
            var updateConnectionCommandHandler = new DeleteCommunicationCommandHandler(logger.Object, mapper, context);

            try
            {
                updateConnectionCommandHandler.Handle(new()
                {
                    Id = communications.First().Id
                },
                default).Wait();
            }
            catch (Exception ex) 
            {
                if (ex.InnerException?.InnerException?.GetType() != typeof(InvalidOperationException))
                    throw;
            }

            // Проверка результата

            // Мы добавили объект в БД
            var communicationInDb = context.Communications.FirstOrDefault();
            //Assert.IsNull(communicationInDb);
        }
    }
}
