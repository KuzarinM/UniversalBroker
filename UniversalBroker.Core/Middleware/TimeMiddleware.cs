using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace UniversalBroker.Core.Middleware
{
    public class TimeMiddleware
    {
        private readonly RequestDelegate _next;

        public TimeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var watch = Stopwatch.StartNew();
            //To add Headers AFTER everything you need to do this
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Append("X-Response-Time-ms", new[] { watch.ElapsedMilliseconds.ToString() });
                watch.Stop();
                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }
}
