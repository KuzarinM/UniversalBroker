using Google.Protobuf;
using MediatR;
using Protos;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Extentions;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Models.Internal;

namespace UniversalBroker.Adapters.Tcp.Logic.Services
{
    public class TcpClientService(
        ILogger<TcpClientService> logger, 
        IInitService initService
        )
    {
        private readonly ILogger _logger = logger;
        private readonly IInitService _initService = initService;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly List<byte> _buffer = new();
        protected SemaphoreSlim SendSemaphore = new(1, 1);
        private bool _listening = false;

        private TcpConfiguration _tcpConfiguration { get; set; }
        private string _path {  get; set; }
        private TcpClient _tcpClient { get; set; }

        public async Task StartWork(TcpClient tcpClient, TcpConfiguration tcpConfiguration, string path, bool needRead = true)
        {
            _tcpClient = tcpClient;
            _tcpConfiguration = tcpConfiguration;
            _path = path;

            if (needRead)
                StartListen();
        }

        public void StartListen()
        {
            if (_listening)
                return;

            _listening = true;
            _ = Task.Run(ListenMessages, _cancellationTokenSource.Token);
        }

        public async Task StopWork()
        {
            await _cancellationTokenSource.CancelAsync();
        }

        public async Task<bool> SendMessage(List<byte> message)
        {
            await SendSemaphore.WaitAsync();
            try
            {
                var ns = _tcpClient.GetStream();
                await ns.WriteAsync(message.ToArray());
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("Клиент разорвал подключение, расходимся");
                return false;
            }
            finally
            {
                SendSemaphore.Release();
            }
        }

        private async Task ListenMessages()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if(await ReadStream(_tcpClient.GetStream()) && !_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var messages = await ExtractMessagesFromBufer();

                        messages.ForEach(async messages => await HandleIncommingMessage(messages));
                    }
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Ошибка при обработке сообщений");
                }
            }
        }

        private async Task<List<List<byte>>> ExtractMessagesFromBufer()
        {
            var res = new List<List<byte>>();

            if (_tcpConfiguration.MessageDevicerByte.HasValue || _tcpConfiguration.MessageFixSize.HasValue)
            {

                int startIndex = 0;
                for (int i = 0; i < _buffer.Count; i++)
                {
                    if(
                        (_tcpConfiguration.MessageFixSize.HasValue && i-startIndex == _tcpConfiguration.MessageFixSize) ||
                        (_tcpConfiguration.MessageDevicerByte.HasValue && _buffer[i] == _tcpConfiguration.MessageDevicerByte)
                        )
                    {
                        res.Add(_buffer.GetRange(startIndex, i-startIndex).ToList());

                        startIndex = i;
                    }
                }

                _buffer.RemoveRange(0, startIndex);
            }
            else if(!string.IsNullOrEmpty(_tcpConfiguration.MessageDeviderRegex))
            {
                var str = Encoding.UTF8.GetString(_buffer.ToArray());

                var messages = Regex.Split(str, _tcpConfiguration.MessageDeviderRegex, RegexOptions.IgnoreCase);

                if (messages.Length > 0)
                {
                    _buffer.Clear();

                    res = messages.Select(x=>Encoding.UTF8.GetBytes(x).ToList()).ToList();
                }
            }

            return res;
        }

        private async Task HandleIncommingMessage(List<byte> messageData)
        {
            var message = new MessageDto()
            {
                Data = ByteString.CopyFrom(messageData.ToArray()),
                Path = _path,
                Headers = {
                    new List<AttributeDto>()
                    {
                        new AttributeDto()
                        {
                            Name = "Custom.ReceiveDateTimeUtc",
                            Value = DateTime.UtcNow.ToString()
                        },
                        new AttributeDto()
                        {
                            Name = "Custom.DataLenth",
                            Value = messageData.Count.ToString()
                        }
                    }
                }
            };

            try
            {
                message.Headers.AddOrUpdateAttribute("Custom.SourceIp", ((IPEndPoint)_tcpClient.Client.RemoteEndPoint!).Address.ToString());
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Не удалось добавить в заголовок сообщения Ip адрес отправлителя");
                message.Headers.AddOrUpdateAttribute("Custom.SourceIp", "Error");
            }

            var task = _initService.GetService?.SendMessage(new()
            {
                Message = message,
            },
            _cancellationTokenSource.Token);

            if (task != null)
                await task;
        }

        private async Task<bool> ReadStream(NetworkStream networkStream)
        {
            bool dataAdded = false;

            int repeatCount = 0;

            try
            {
                while (networkStream.DataAvailable && !_cancellationTokenSource.IsCancellationRequested)
                {
                    byte[] tmp = new byte[1024];

                    var count = await networkStream.ReadAsync(tmp, _cancellationTokenSource.Token);

                    if (count > 0)
                    {
                        _buffer.AddRange(tmp.Take(count));
                        dataAdded = true;
                        repeatCount++;
                    }

                    // Защитный механизм. Если сообщения прут прям потоком, то мы должны иногда отвлекаться
                    if (repeatCount > 10)
                        break;
                }
            }
            catch (IOException)
            {
                _logger.LogInformation("Поток был отменён");
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при чтении потока");
            }
            return dataAdded;
        }
    }
}
