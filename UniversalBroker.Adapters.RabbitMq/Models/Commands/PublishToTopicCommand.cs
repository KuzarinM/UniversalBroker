using MediatR;
using Protos;

namespace UniversalBroker.Adapters.RabbitMq.Models.Commands
{
    /// <summary>
    /// Отправка сообщения в топик
    /// </summary>
    public class PublishToTopicCommand: IRequest
    {
        /// <summary>
        /// Само сообщение
        /// </summary>
        public MessageDto Message { get; set; }
    }
}
