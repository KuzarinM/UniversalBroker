using Microsoft.Extensions.Logging;
using Moq;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Handlers.Commands.Communications;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Communications
{
    [TestClass]
    public class CommunicationSetAttributeTest
    {
        [TestMethod]
        public void AttributeUpdateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<CommunicationSetAttributeCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            var manager = new Mock<AbstractAdaptersManager>();
            var service = new Mock<IAdapterCoreService>();
            using var context = MockExtentions.GetEmptyFullDbContext();

            CoreMessage? message = null;

            // Добавление моков того, что может понадобится
            service.Setup(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()))
                    .Callback<CoreMessage, CancellationToken>((x, _) => message = x)
                    .Returns(Task.Delay(100));
            manager.Setup(x => x.GetAdapterById(It.IsAny<Guid>())).Returns(service.Object);

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
            var updateAttr = new Dictionary<string, string?>()
                {
                    {"123", "123" },
                    {"456", "456" }
                };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            // Тест
            var updateConnectionCommandHandler = new CommunicationSetAttributeCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = updateConnectionCommandHandler.Handle(new()
            {
                CommunicationId = communications.First().Id,
                Attributes = updateAttr
            },
            default).Result;

            // Проверка результата

            // Мы отправили сообщение
            service.Verify(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()));

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В сообщении всё что надо есть
            Assert.IsTrue(
                res.Attributes.Count == updateAttr.Count);

            // Мы добавили объект в БД
            var communicationInDb = context.Communications.FirstOrDefault();
            Assert.IsNotNull(communicationInDb);

            // Имя и путь совпали
            Assert.IsTrue(communicationInDb!.CommunicationAttributes.Count == updateAttr.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось обновить атрибуты у Соединения которого нет")]
        public void CommunicationNotExistTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<CommunicationSetAttributeCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            var manager = new Mock<AbstractAdaptersManager>();
            var service = new Mock<IAdapterCoreService>();
            using var context = MockExtentions.GetEmptyFullDbContext();

            CoreMessage? message = null;

            // Добавление моков того, что может понадобится
            service.Setup(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()))
                    .Callback<CoreMessage, CancellationToken>((x, _) => message = x)
                    .Returns(Task.Delay(100));
            manager.Setup(x => x.GetAdapterById(It.IsAny<Guid>())).Returns(service.Object);

            // Тестовые данные
            var updateAttr = new Dictionary<string, string?>()
                {
                    {"123", "123" },
                    {"456", "456" }
                };

            // Тест
            var updateConnectionCommandHandler = new CommunicationSetAttributeCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = updateConnectionCommandHandler.Handle(new()
            {
                CommunicationId = Guid.NewGuid(),
                Attributes = updateAttr
            },
            default).Result;
        }
    }
}
