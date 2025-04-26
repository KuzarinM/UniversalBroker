using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Chanels
{
    [TestClass]
    public class UpdateChanelTests
    {
        [TestMethod]
        public void SimpleUpdateTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>() 
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = "testNew",
                    Script = "newScript"
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Id == updateDto.Id);
            Assert.IsNotNull(record);

            // Возврат соответсвует требованиям
            Assert.IsTrue( res.Name == updateDto.UpdateDto.Name
                && res.Id == record.Id
            );
            Assert.IsTrue(res.Script == updateDto.UpdateDto.Script);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось обновить канал, id которого нет")]
        public void NotExistsTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = Guid.NewGuid(),
                UpdateDto = new()
                {
                    Name = "testNew",
                    Script = "newScript"
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;
        }

        [TestMethod]
        public void UpdateConnectionsFromEmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = chanels.First().Name,
                    Script = chanels.First().Script.Path,
                    InputConnections = [connections.Where(x=>x.Isinput).Last().Id],
                    OutputConnections = [connections.Where(x => !x.Isinput).Last().Id]
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Id == updateDto.Id);
            Assert.IsNotNull(record);

            // Ничего не поменялось
            Assert.IsTrue(res.Name == updateDto.UpdateDto.Name
                && res.Id == record.Id
            );

            // Ничего не поменялось
            Assert.IsTrue(
                res.InputConnections.Count() == 1
                && res.InputConnections.First() == connections.Where(x => x.Isinput).Last().Id
            );

            Assert.IsTrue(
                res.OutputConnections.Count() == 1
                && res.OutputConnections.First() == connections.Where(x => !x.Isinput).Last().Id
            );
        }

        [TestMethod]
        public void UpdateConnectionsToEmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First(),
                    Connections = [connections.Where(x=>x.Isinput).First(), connections.Where(x=>!x.Isinput).First()]
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = chanels.First().Name,
                    Script = chanels.First().Script.Path,
                    InputConnections = [],
                    OutputConnections = [],
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Id == updateDto.Id);
            Assert.IsNotNull(record);

            // Ничего не поменялось
            Assert.IsTrue(res.Name == updateDto.UpdateDto.Name
                && res.Id == record.Id
            );

            // Ничего не поменялось
            Assert.IsTrue(
                res.InputConnections.Count() == 0
            );

            Assert.IsTrue(
                res.OutputConnections.Count() == 0
            );
        }

        [TestMethod]
        public void UpdateChanelsFromEmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script1"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "456",
                    Script = scripts.Last()
                }
            };
            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = chanels.First().Name,
                    Script = chanels.First().Script.Path,
                    OutputChanels = [chanels.Last().Id]
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Id == updateDto.Id);
            Assert.IsNotNull(record);

            // Ничего не поменялось
            Assert.IsTrue(res.Name == updateDto.UpdateDto.Name
                && res.Id == record.Id
            );

            // Ничего не поменялось
            Assert.IsTrue(
                res.OutputChanels.Count() == 1
                && res.OutputChanels.First() == chanels.Last().Id
            );
        }

        [TestMethod]
        public void UpdateChanelsToEmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<UpdateChanelCommandHandler>>();
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
            var scripts = new List<Script>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Path = "script1"
                }
            };
            var chanels = new List<Chanel>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    Script = scripts.First()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "456",
                    Script = scripts.Last()
                }
            };
            chanels.First().ToChanels = [chanels.Last()];

            var updateDto = new UpdateChanelCommand()
            {
                Id = chanels.First().Id,
                UpdateDto = new()
                {
                    Name = chanels.First().Name,
                    Script = chanels.First().Script.Path,
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.Connections.AddRange(connections);
            context.Chanels.AddRange(chanels);
            context.Scripts.AddRange(scripts);
            context.SaveChanges();

            // Тест
            var addChanelCommandHandler = new UpdateChanelCommandHandler(logger.Object, mapper, context);
            var res = addChanelCommandHandler.Handle(updateDto,
            default).Result;

            //Мы что-то добавили
            Assert.IsNotNull(res);

            // В БД появилась запись
            var record = context.Chanels.FirstOrDefault(x => x.Id == updateDto.Id);
            Assert.IsNotNull(record);

            // Ничего не поменялось
            Assert.IsTrue(res.Name == updateDto.UpdateDto.Name
                && res.Id == record.Id
            );

            // Ничего не поменялось
            Assert.IsTrue(
                res.OutputChanels.Count() == 0
            );
        }
    }
}
