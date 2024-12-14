using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UniversalBroker.Core.Models.Internals
{
    public class ScriptExecutionLog
    {
        public Guid ScriptId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime Created {  get; set; } = DateTime.UtcNow;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
