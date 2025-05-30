using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBroker.Tests.Performance
{
    public class TcpAckPerformanceTest() : AbstractPreformanceTest("Отправка подтверждения в СОИ")
    {
        public string TcpIp = "127.0.0.1";
        public int Port = 9999;
        public string Message = "test";


        protected override async Task<double> Test()
        {
            var client = new TcpClient(TcpIp, Port);

            var task = new TaskCompletionSource<(string, DateTime)>();

            _ = Task.Run(() => StartListenAsync(client, task));

            var sendTime = await SendTcpMessage(client, Message);

            var receiveTime = await task.Task;

            client.Close();

            return (receiveTime.Item2 - sendTime).TotalMilliseconds;
        }

        private async Task<DateTime> SendTcpMessage(TcpClient client, string message)
        {
            var binaryMessage = Encoding.UTF8.GetBytes(message);

            await client.GetStream().WriteAsync(binaryMessage);

            var time = DateTime.UtcNow;

            return time;
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
                    string message = Encoding.UTF8.GetString(buffer.ToArray()).Replace("Status", "");

                    if (message.Contains("ACK"))
                    {
                        var date = DateTime.UtcNow;

                        task.SetResult((message, date));
                        break;
                    }

                    if (buffer.Count() > 1000)
                    {
                        buffer.Clear();
                    }
                }
            }
        }
    }
}
