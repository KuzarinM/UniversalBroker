using Protos;
using System.Collections.Concurrent;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Services;

namespace UniversalBroker.Adapters.Tcp.Models.Internal
{
    public class TcpServerModel
    {
        public ConnectionDto? InConnection { get; set; }
        public ConnectionDto? OutConnection { get; set; }

        public TcpListener TcpListener { get; set; }

        public TcpConfiguration TcpConfiguration { get; set; }

        public Task<TcpClient> ReceiveClientTask { get; set; }

        public List<ITcpClientService> Clients { get; set; } = new();
    }
}
