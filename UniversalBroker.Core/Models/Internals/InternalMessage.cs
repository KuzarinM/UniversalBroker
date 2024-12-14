using System.Collections.Generic;
using System.Text;

namespace UniversalBroker.Core.Models.Internals
{
    /// <summary>
    /// Представляет внутреннее сообщение, используемое для коммуникации внутри системы.
    /// </summary>
    public record InternalMessage
    {
        /// <summary>
        /// Уникальный идентификатор для внутреннего сообщения.
        /// </summary>
        public Guid InternalId { get; set; }

        /// <summary>
        /// Указывает, является ли сообщение исходящим от соединения.
        /// </summary>
        public bool IsFromConnection { get; set; }

        /// <summary>
        /// Идентификатор источника, который отправил сообщение.
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// Данные сообщения в формате байтов.
        /// </summary>
        public List<byte> Data { get; set; }

        /// <summary>
        /// Данные сообщения, декодированные в виде строки UTF-8.
        /// </summary>
        public string Text => Encoding.UTF8.GetString(Data.ToArray());

        /// <summary>
        /// Словарь, содержащий информацию заголовков, связанную с сообщением.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();
    }
}