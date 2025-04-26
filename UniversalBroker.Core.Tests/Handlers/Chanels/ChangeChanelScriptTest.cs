using Microsoft.EntityFrameworkCore;
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
using UniversalBroker.Core.Tests.Core;

namespace UniversalBroker.Core.Tests.Handlers.Chanels
{
    [TestClass]
    public class ChangeChanelScriptTest
    {
        [TestMethod]
        public void PositiveTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<ChangeChanelScriptCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные
           
            var scripts = new List<Script>()
            {
                new()
                {
                    Id= Guid.NewGuid(),
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
            // вставка тестовых данных
            context.Scripts.AddRange(scripts);
            context.Chanels.AddRange(chanels);
            context.SaveChanges();

            var changeScript = new ChangeChanelScriptCommand()
            {
                Id = chanels.First().Id,
                ScriptText = "script2"
            };

            // Тест
            var addChanelCommandHandler = new ChangeChanelScriptCommandHandler(logger.Object, mapper, context);
            addChanelCommandHandler.Handle(changeScript,
            default).Wait();

            // В БД появилась запись
            var record = context.Chanels.Include(x=>x.Script).FirstOrDefault();
            Assert.IsNotNull(record);

            Assert.IsNotNull(record.Script);

            // Скрипт поменялся
            Assert.IsTrue(record.Id == chanels.First().Id && record.Script.Path == changeScript.ScriptText);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException), "Удалось изменить текст скрипта для несуществующего Канала")]
        public void NotExistsTest()
        {
            // Создание всех моков
            var logger = new Mock<ILogger<ChangeChanelScriptCommandHandler>>();
            var mapper = MockExtentions.GetMapper();
            using var context = MockExtentions.GetEmptyFullDbContext();

            // Тестовые данные

            var scripts = new List<Script>()
            {
                new()
                {
                    Id= Guid.NewGuid(),
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
            // вставка тестовых данных
            context.Scripts.AddRange(scripts);
            context.Chanels.AddRange(chanels);
            context.SaveChanges();

            var changeScript = new ChangeChanelScriptCommand()
            {
                Id = Guid.NewGuid(),
                ScriptText = "script2"
            };

            // Тест
            var addChanelCommandHandler = new ChangeChanelScriptCommandHandler(logger.Object, mapper, context);
            addChanelCommandHandler.Handle(changeScript,
            default).Wait();
        }
    }
}
