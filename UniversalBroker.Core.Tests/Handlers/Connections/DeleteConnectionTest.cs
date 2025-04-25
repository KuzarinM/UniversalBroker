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
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Connections
{
    [TestClass]
    public class DeleteConnectionTest
    {
        [TestMethod]
        public void PositiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<DeleteConnectionCommandHandler>>();
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
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test",
                    Path = "/test",
                    Isinput = true,
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var updateConnectionCommandHandler = new DeleteConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            updateConnectionCommandHandler.Handle(new()
            {
                ConnectionId = connections.First().Id,
            },
            default).Wait();

            // Проверка результата

            // Мы отправили сообщение
            service.Verify(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()));

            // Сообщенние не null
            Assert.IsNotNull(message);

            // В сообщении всё что надо есть
            Assert.IsTrue(message.DeletedConnection != null && message.DeletedConnection.Id == connections.First().Id.ToString());

            // Мы добавили объект в БД
            var connectionInDb = context.Connections.FirstOrDefault();
            Assert.IsNull(connectionInDb);
        }
    }
}
