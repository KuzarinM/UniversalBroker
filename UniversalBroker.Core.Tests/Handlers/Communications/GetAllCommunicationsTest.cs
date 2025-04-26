using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Handlers.Commands.Communications;
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Logic.Handlers.Queries.Communication;
using UniversalBroker.Core.Models.Queries.Communications;
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Communications
{
    [TestClass]
    public class GetAllCommunicationsTest
    {
        [TestMethod]
        public void PaginationTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetAllCommunicationsQueryHandler>>();
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

            GetAllCommunicationsQuery query = new()
            {
                PageNumber = 0,
                PageSize = 1
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetAllCommunicationsQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNotNull(res);

            //Проверка показателей страницы
            Assert.IsTrue(
                res.PageSize == query.PageSize
                && res.CurrentPage == query.PageNumber
                && res.TotalPages == (int)(communications.Count() / query.PageSize + 0.5f)
                && res.PageSize >= res.Page.Count()
            );

            // Проверка состава стараницы
            Assert.IsTrue(
                res.Page.All(x=>communications.Any(y=>y.Id == x.Id && y.Name == x.Name && y.Status == x.Status && y.Description == x.Description))
            );
        }

        [TestMethod]
        public void FiltratioTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetAllCommunicationsQueryHandler>>();
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
                    Status = true,
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

            GetAllCommunicationsQuery query = new()
            {
                PageNumber = 0,
                PageSize = 100,
                Status = true,
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetAllCommunicationsQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNotNull(res);

            // Проверка колличества элементов на старанице
            Assert.IsTrue(
                res.Page.Count() == communications.Where(x => x.Status).Count()
            );

            // Проверка состава стараницы
            Assert.IsTrue(
                res.Page.All(x => communications.Where(x=>x.Status).Any(y => y.Id == x.Id && y.Name == x.Name && y.Status == x.Status && y.Description == x.Description))
            );
        }

        [TestMethod]
        public void TextSearchTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetAllCommunicationsQueryHandler>>();
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
                    Status = true,
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "tes3",
                    Name = "test3",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            GetAllCommunicationsQuery query = new()
            {
                PageNumber = 0,
                PageSize = 100,
                NameSearch = "est"
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetAllCommunicationsQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNotNull(res);

            // Проверка колличества элементов на старанице
            Assert.IsTrue(
                res.Page.Count() == communications.Where(x => x.Name.Contains(query.NameSearch)).Count()
            );

            // Проверка состава стараницы
            Assert.IsTrue(
                res.Page.All(x => communications.Where(x => x.Name.Contains(query.NameSearch)).Any(y => y.Id == x.Id && y.Name == x.Name && y.Status == x.Status && y.Description == x.Description))
            );
        }

        [TestMethod]
        public void EmptyTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<GetAllCommunicationsQueryHandler>>();
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
                    Status = true,
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
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Status = false,
                    Description = "tes3",
                    Name = "test3",
                    TypeIdentifier = Guid.NewGuid(),
                }
            };

            // вставка тестовых данных
            context.Communications.AddRange(communications);
            context.SaveChanges();

            GetAllCommunicationsQuery query = new()
            {
                PageNumber = 0,
                PageSize = 100,
                Status = true,
                NameSearch = "1"
            };

            // Тест
            var getAllCommunicationsQueryHandler = new GetAllCommunicationsQueryHandler(logger.Object, mapper, context);

            var res = getAllCommunicationsQueryHandler.Handle(query,
            default).Result;

            // Проверка результата
            Assert.IsNotNull(res);

            // Проверка колличества элементов на старанице
            Assert.IsTrue(
                res.Page.Count() == 0
            );
        }
    }
}
