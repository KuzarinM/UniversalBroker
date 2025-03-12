using Protos;
using System.Collections.Concurrent;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Services;

namespace UniversalBroker.Adapters.Tcp.Models.Internal
{
    public class TcpClientModel
    {
        public ConnectionDto? InConnection { get; set; }
        public ConnectionDto? OutConnection { get; set; }

        public TcpConfiguration TcpConfiguration { get; set; }

        public ITcpClientService Client { get; set; }
    }
}
