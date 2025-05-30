using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalBroker.Tests.Performance
{
    public class RabbitToTcpPerformanceTest() : AbstractPreformanceTest("Передача из ССИ в СОИ")
    {
        private ConnectionFactory _connectionConfig;

        public string TcpIp = "127.0.0.1";
        public int Port = 9999;
        public string QueueName = "test";
        public string Message = "test";
        public string RabbitHostName = "localhost";
        public string RabbitUserName = "rabbit";
        public string RabbitPassword = "rabbit";

        protected override async Task<double> Test()
        {
            _connectionConfig = new ConnectionFactory()
            {
                HostName = RabbitHostName,
                UserName = RabbitUserName,
                Password = RabbitPassword,
            };

            var receiveTask = ReceiveMessageFromTcp(TcpIp, Port);

            var sendTime = await SendRabbitMqMessage(QueueName, Message);

            var receive = await receiveTask;

            return (receive.Item2-sendTime).TotalMilliseconds;
        }

        private Task<(string, DateTime)> ReceiveMessageFromTcp(string ip, int port)
        {
            var client = new TcpClient(ip, port);

            var task = new TaskCompletionSource<(string, DateTime)>();

            Task.Run(() => StartListenAsync(client, task));

            return task.Task;
        }

        private async Task StartListenAsync(TcpClient client, TaskCompletionSource<(string, DateTime)> task)
        {
            List<byte> buffer = new List<byte>();

            while (client.Connected) 
            {
                var networkStream = client.GetStream();
                int repeatCount = 0;

                try
                {
                    while (networkStream.DataAvailable)
                    {
                        byte[] tmp = new byte[1024];

                        var count = networkStream.Read(tmp);

                        if (count > 0)
                        {
                            buffer.AddRange(tmp.Take(count));
                            repeatCount++;
                        }

                        // Защитный механизм. Если сообщения прут прям потоком, то мы должны иногда отвлекаться
                        if (repeatCount > 10)
                            break;
                    }
                }
                catch (IOException)
                {
                }
                catch (Exception ex)
                {
                }

                if (buffer.Any())
                {
                    string message = Encoding.UTF8.GetString(buffer.ToArray()).Replace("Status","");

                    if (message.Length > 10)
                    {
                        var date = DateTime.UtcNow;

                        task.SetResult((message, date));
                        break;
                    }

                    if(buffer.Count() > 1000)
                    {
                        buffer.Clear();
                    }
                }
            }

            client.Close();
        }

        private async Task<DateTime> SendRabbitMqMessage(string queue, string message)
        {
            var connection = await _connectionConfig.CreateConnectionAsync();

            using var channel = await connection.CreateChannelAsync();

            var messageBytes = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                body: messageBytes
            );

            return DateTime.UtcNow;
        }
    }
}
