using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Extentions;
using Attribute = UniversalBroker.Core.Database.Models.Attribute;

namespace UniversalBroker.Core.Tests.Core
{
    public static class MockExtentions
    {
        public static BrockerContext GetEmptyFullDbContext()
        {
            var options = new DbContextOptionsBuilder<BrockerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Это чтобы можно было паралеььно запускать
            .Options;

            return new BrockerContext(options);
        }

        public static IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfiles());
            });

            return mapperConfig.CreateMapper();
        }
    }
}
