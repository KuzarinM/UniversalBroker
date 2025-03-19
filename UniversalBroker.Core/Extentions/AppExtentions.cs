using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Middleware;

namespace UniversalBroker.Core.Extentions
{
    public static class AppExtentions
    {
        public static WebApplication AddMiddlewares(this WebApplication app) 
        {
            app.UseMiddleware<SwaggerServerMiddleware>();
            app.UseMiddleware<TimeMiddleware>();

            return app;
        }

        public static WebApplication AddSwagger(this WebApplication app) 
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            return app;
        }

        public static WebApplication ClearCommunications(this WebApplication app)
        {
            var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<BrockerContext>();

            context.Communications.ExecuteUpdate(x => x.SetProperty(y => y.Status, false));

            return app;
        }
    }
}
