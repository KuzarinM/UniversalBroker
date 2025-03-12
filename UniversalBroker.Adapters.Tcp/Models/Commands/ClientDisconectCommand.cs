using MediatR;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Logic.Services;

namespace UniversalBroker.Adapters.Tcp.Models.Commands
{
    public class ClientDisconectCommand: IRequest
    {
        public string Path { get; set; }

        public TcpClientService Client { get; set; }
    }
}
