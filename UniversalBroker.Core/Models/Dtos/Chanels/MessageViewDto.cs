using System.Text;
using UniversalBroker.Core.Models.Enums;

namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Dto для сообщения
    /// </summary>
    public class MessageViewDto
    {
        /// <summary>
        /// Бинарные данные
        /// </summary>
        public List<byte> Data { get; set; } = new();

        /// <summary>
        /// Тест сообщения
        /// </summary>
        public string Text => Encoding.UTF8.GetString(Data.ToArray());

        /// <summary>
        /// Выремя обработки
        /// </summary>
        public DateTime Datetime { get; set; }

        /// <summary>
        /// Откуда
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Имя того, откуда
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// Куда
        /// </summary>
        public Guid TargetId { get; set; }

        /// <summary>
        /// Имя того, куда
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Какой тип передачи
        /// </summary>
        public MessageDirection Direction { get; set; }

        /// <summary>
        /// Заголовки
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}
