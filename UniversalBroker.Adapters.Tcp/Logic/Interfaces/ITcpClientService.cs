using System.Net.Sockets;
using System.Threading.Tasks;
using UniversalBroker.Adapters.Tcp.Configurations;

namespace UniversalBroker.Adapters.Tcp.Logic.Interfaces
{
    /// <summary>
    /// Сервис для работы с клиентом
    /// </summary>
    public interface ITcpClientService
    {
        /// <summary>
        /// Запустить сервис и начать слушать (если надо)
        /// </summary>
        /// <param name="tcpClient">Сам клиент</param>
        /// <param name="tcpConfiguration">Конфигурация</param>
        /// <param name="path">Путь</param>
        /// <param name="needRead">Нужно ли запускать читателя</param>
        /// <returns></returns>
        Task StartWork(TcpClient tcpClient, TcpConfiguration tcpConfiguration, string path, bool needRead = true);

        /// <summary>
        /// Старт чтения, если такового не было до этого
        /// </summary>
        void StartListen();

        /// <summary>
        /// Остановить чтение. Но при этом можно его перезапустить
        /// </summary>
        void StopListen();

        /// <summary>
        /// Остановить работу системы кординально
        /// </summary>
        /// <returns></returns>
        Task StopWork();

        /// <summary>
        /// Отправить сообщение по TCP
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> SendMessage(List<byte> message);
    }
}
