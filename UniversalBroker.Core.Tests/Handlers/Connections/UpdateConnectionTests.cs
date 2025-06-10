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
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Tests.Core;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Connection = UniversalBroker.Core.Database.Models.Connection;

namespace UniversalBroker.Core.Tests.Handlers.Connections
{
    [TestClass]
    public class UpdateConnectionTests
    {
        [TestMethod]
        public void SimpleUpdateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateConnectionCommandHandler>>();
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
            var updateDto = new UpdateConnectionDto()
            {
                Name = "test1",
                Path = "/test1",
                Attribues = connections.First().ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var updateConnectionCommandHandler = new UpdateConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = updateConnectionCommandHandler.Handle(new()
            {
                ConnectionId = connections.First().Id,
                NeedNotifyAdapter = true,
                UpdateDto = updateDto
            },
            default).Result;

            // Проверка результата

            // Мы отправили сообщение
            service.Verify(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()));

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В сообщении всё что надо есть
            Assert.IsTrue(
                res.Name == updateDto.Name
                && res.Path == updateDto.Path
                && res.Attribues.Count == updateDto.Attribues.Count);

            // Мы добавили объект в БД
            var connectionInDb = context.Connections.FirstOrDefault();
            Assert.IsNotNull(connectionInDb);

            // Имя и путь совпали
            Assert.IsTrue(connectionInDb!.Name == updateDto.Name && connectionInDb!.Path == updateDto.Path);
        }

        [TestMethod]
        public void AttributeUpdateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateConnectionCommandHandler>>();
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
            var updateDto = new UpdateConnectionDto()
            {
                Name = connections.First().Name,
                Path = connections.First().Path,
                Attribues = connections.First().ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)
            };

            updateDto.Attribues.Add("123", "123");

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var updateConnectionCommandHandler = new UpdateConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = updateConnectionCommandHandler.Handle(new()
            {
                ConnectionId = connections.First().Id,
                NeedNotifyAdapter = true,
                UpdateDto = updateDto
            },
            default).Result;

            // Проверка результата

            // Мы отправили сообщение
            service.Verify(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()));

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В сообщении всё что надо есть
            Assert.IsTrue(
                res.Attribues.Count == updateDto.Attribues.Count);

            // Мы добавили объект в БД
            var connectionInDb = context.Connections.FirstOrDefault();
            Assert.IsNotNull(connectionInDb);

            // Имя и путь совпали
            Assert.IsTrue(connectionInDb!.ConnectionAttributes.Count() == updateDto.Attribues.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось обновить Подключение которого нет в БД")]
        public void UpdateNotExistingTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateConnectionCommandHandler>>();
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
            var updateDto = new UpdateConnectionDto()
            {
                Name = "test1",
                Path = "/test1",
                Attribues = connections.First().ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            // Тест
            var updateConnectionCommandHandler = new UpdateConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = updateConnectionCommandHandler.Handle(new()
            {
                ConnectionId = connections.First().Id,
                NeedNotifyAdapter = true,
                UpdateDto = updateDto
            },
            default).Result;
        }
    }
}
