using Google.Protobuf.Collections;
using MediatR;
using Protos;

namespace UniversalBroker.Adapters.RabbitMq.Models.Commands
{
    /// <summary>
    /// Подписываемся на топик по пути и с аргументами.
    /// </summary>
    public class SubscribeOnTopicCommand: IRequest
    {
        /// <summary>
        /// Само подключение, на основе которого создаёмся
        /// </summary>
        public ConnectionDto Connection { get; set; }
    }
}
