using Swashbuckle.AspNetCore.Swagger;
using UniversalBroker.Core.Extentions;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddServices();

builder.AddLogger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

app.UseMiddleware<SwaggerServerMiddleware>();
app.UseMiddleware<TimeMiddleware>(); 


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var tokenSource = new CancellationTokenSource();

await app.Services.GetRequiredService<IDbLogingService>().StartLogging(tokenSource.Token);

app.Run();
