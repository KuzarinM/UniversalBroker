using Microsoft.Extensions.Logging;
using Moq;
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
    public class AddChanelTest
    {
        [TestMethod]
        public void AddingSimpleTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Name == createDto.Name);
            Assert.IsNotNull(record);

            // Возврат соответсвует требованиям
            Assert.IsTrue(res.Script == createDto.Script 
                && res.Name == createDto.Name 
                && res.Id == record.Id 
                && res.InputConnections.Count() == 0
                && res.OutputConnections.Count() == 0
                && res.OutputChanels.Count() == 0
            );
        }

        [TestMethod]
        public void AddingWithConnectionTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                InputConnections = [connections.Where(x=>x.Isinput).First().Id],
                OutputConnections = [connections.Where(x => !x.Isinput).First().Id],
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Name == createDto.Name);
            Assert.IsNotNull(record);

            // Возврат соответсвует требованиям
            Assert.IsTrue(res.Script == createDto.Script
                && res.Name == createDto.Name
                && res.Id == record.Id
                && res.InputConnections.Count() == 1
                && res.OutputConnections.Count() == 1
                && res.OutputChanels.Count() == 0
            );

            //Соединения соответсвуют требованиям
            Assert.IsTrue(
                res.InputConnections.First() == connections.Where(x => x.Isinput).First().Id
                && res.OutputConnections.First() == connections.Where(x => !x.Isinput).First().Id);
        }

        [TestMethod]
        public void AddingWithChanelTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123"
                }
            };
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                OutputChanels = [chanels.First().Id]
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Name == createDto.Name);
            Assert.IsNotNull(record);

            // Возврат соответсвует требованиям
            Assert.IsTrue(res.Script == createDto.Script
                && res.Name == createDto.Name
                && res.Id == record.Id
                && res.InputConnections.Count() == 0
                && res.OutputConnections.Count() == 0
                && res.OutputChanels.Count() == 1
            );

            //Соединения соответсвуют требованиям
            Assert.IsTrue(res.OutputChanels.First() == chanels.First().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Канал с выходным подключением как входом")]
        public void AddingInputAsOutputTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                InputConnections = [connections.Where(x => !x.Isinput).First().Id],
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Канал с выходным подключением как выходом")]
        public void AddingOutputAsInputTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var connections = new List<Connection>()
            {
                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test1",
                    Path = "/test2",
                    Isinput = true,
                },
                                new Connection()
                {
                    Id = Guid.NewGuid(),
                    CommunicationId = communications.First().Id,
                    Name = "Test2",
                    Path = "/test2",
                    Isinput = false,
                },
            };
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                OutputConnections = [connections.Where(x => x.Isinput).First().Id],
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Канал с несуществующим подключением")]
        public void AddingWithNotExistingConnectionTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                InputConnections = [Guid.NewGuid()]
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось создать Канал с несуществующим подключением")]
        public void AddingWithNotExistingChannelTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<AddChanelCommandHandler>>();
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
            var createDto = new CreateChanelDto()
            {
                Name = "Test",
                Script = "script",
                OutputChanels = [Guid.NewGuid()]
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new AddChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(new()
            {
                CreateChanelDto = createDto
            },
            default).Result;
        }
    }
}
