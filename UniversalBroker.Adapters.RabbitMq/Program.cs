using PIHelperSh.Configuration;
using UniversalBroker.Adapters.RabbitMq.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.AddLogger();

builder.Services.AddConfigurations(builder.Configuration);
builder.Configuration.AddConstants();

builder.Services.AddServices();

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
