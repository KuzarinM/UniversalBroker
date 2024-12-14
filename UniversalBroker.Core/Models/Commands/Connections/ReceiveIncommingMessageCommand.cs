using MediatR;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class ReceiveIncommingMessageCommand: IRequest
    {
        public Guid CommunicationId { get; set; }

        public List<byte> Data { get; set; } = new();

        public string Path { get; set; } = string.Empty;

        public Dictionary<string, string> Headers = new();
    }
}
