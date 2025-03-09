using Swashbuckle.AspNetCore.Swagger;
using UniversalBroker.Core.Extentions;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Services;
using UniversalBroker.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddServices();

builder.AddLogger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

app.AddMiddlewares();

app.AddSwagger();

app.ClearCommunications();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<CoreGrpcService>();

app.Run();
