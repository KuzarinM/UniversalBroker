using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Логи скприта из канала
    /// </summary>
    public class ChanelScriptLogDto
    {
        /// <summary>
        /// Выремя лога
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Уровень логирования
        /// </summary>
        public LogLevel Lavel {  get; set; }

        /// <summary>
        /// Текст логов
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
