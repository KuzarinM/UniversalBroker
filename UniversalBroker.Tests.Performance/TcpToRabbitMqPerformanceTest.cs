using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalBroker.Tests.Performance
{
    public class TcpToRabbitMqPerformanceTest() : AbstractPreformanceTest("Передача из СОИ в ССИ")
    {
        private ConnectionFactory _connectionConfig;

        public string TcpIp = "127.0.0.1";
        public int Port = 9999;
        public string QueueName = "test";
        public string Message = "test";
        public string RabbitHostName = "localhost";
        public string RabbitUserName = "rabbit";
        public string RabbitPassword = "rabbit";

        protected async override Task<double> Test()
        {
            _connectionConfig = new ConnectionFactory()
            {
                HostName = RabbitHostName,
                UserName = RabbitUserName,
                Password = RabbitPassword,
            };

            var receiveTask = await ReceiveMessageFromRabbit(QueueName);

            var sendTime = await SendTcpMessage(TcpIp,Port,Message);

            var receive = await receiveTask;

            return (receive.Item2 - sendTime).TotalMilliseconds;
        }

        private async Task<DateTime> SendTcpMessage(string ip, int port, string message)
        {
            var client = new TcpClient(ip, port);

            var binaryMessage = Encoding.UTF8.GetBytes(message);

            await client.GetStream().WriteAsync(binaryMessage);

            var time = DateTime.UtcNow;

            client.Close();

            return time;
        }

        private async Task<Task<(string, DateTime)>> ReceiveMessageFromRabbit(string queue)
        {
            var task = new TaskCompletionSource<(string, DateTime)>();

            var connection = await _connectionConfig.CreateConnectionAsync();

            var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var time = DateTime.UtcNow;

                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                task.SetResult((message, time));

                channel.Dispose();
            };

            await channel.BasicConsumeAsync(
                    queue,
                    autoAck: true,
                    consumer: consumer
                    );

            return task.Task;
        }
    }
}
