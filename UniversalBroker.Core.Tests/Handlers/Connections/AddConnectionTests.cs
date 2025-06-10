using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Extentions;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Connections
{
    [TestClass]
    // Тестируем AddConnectionCommandHandler
    public class AddConnectionTests
    {
        [TestMethod]
        public void PoritiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddConnectionCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            var manager = new Mock<AbstractAdaptersManager>();
            var service = new Mock<IAdapterCoreService>();
            using var context = MockExtentions.GetEmptyFullDbContext();

            CoreMessage? message = null;

            // Добавление моков того, что может понадобится
            service.Setup(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()))
                    .Callback<CoreMessage,CancellationToken>( (x, _) => message = x)
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
            var connectionDto = new CreateConnectionDto()
            {
                Name = "Test",
                Path = "/test",
                Attribues = new()
                    {
                        {"key1", "value1" }
                    },
                IsInput = true,
                CommunicationId = communications.First().Id,
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            // Тест
            var addConnectionCommandHandler = new AddConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = addConnectionCommandHandler.Handle(new()
            {
                ConnectionDto = connectionDto
            },
            default).Result;

            // Мы отправили сообщение
            service.Verify(x => x.SendMessage(It.IsAny<CoreMessage>(), It.IsAny<CancellationToken>()));

            // Сообщенние не null
            Assert.IsNotNull(res);

            // В сообщении всё что надо есть
            Assert.IsTrue(
                res.IsInput == connectionDto.IsInput 
                && res.Name == connectionDto.Name 
                && res.Path == connectionDto.Path 
                && res.Attribues.Count == connectionDto.Attribues.Count);

            // Заголовки те же
            Assert.IsTrue(connectionDto.Attribues.All(x=>res.Attribues.ContainsKey(x.Key) && res.Attribues[x.Key] == x.Value));

            // Мы добавили объект в БД
            var connectionInDb = context.Connections.FirstOrDefault();
            Assert.IsNotNull(connectionInDb);

            // Имя и путь совпали
            Assert.IsTrue(connectionInDb!.Name == connectionDto.Name && connectionInDb!.Path == connectionDto.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Подключение без соединения")]
        public void NegativeCommunicationIdTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddConnectionCommandHandler>>();
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
            var connectionDto = new CreateConnectionDto()
            {
                Name = "Test",
                Path = "/test",
                Attribues = new()
                    {
                        {"key1", "value1" }
                    },
                IsInput = true,
                CommunicationId = communications.First().Id,
            };

            // Тестовые данные не вставляются

            // Тест
            var addConnectionCommandHandler = new AddConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = addConnectionCommandHandler.Handle(new()
            {
                ConnectionDto = connectionDto
            },
            default).Result;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Подключение с именем которое уже есть")]
        public void NegativeConnectioExistTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddConnectionCommandHandler>>();
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
            var connectionDto = new CreateConnectionDto()
            {
                Name = connections.First().Name,
                Path = "/test",
                Attribues = new()
                    {
                        {"key1", "value1" }
                    },
                IsInput = true,
                CommunicationId = communications.First().Id,
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addConnectionCommandHandler = new AddConnectionCommandHandler(logger.Object, mapper, context, manager.Object);
            var res = addConnectionCommandHandler.Handle(new()
            {
                ConnectionDto = connectionDto
            },
            default).Result;
        }
    }
}
