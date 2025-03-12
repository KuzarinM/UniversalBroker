using System.Collections.Concurrent;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Models.Internal;

namespace UniversalBroker.Adapters.Tcp.Logic.Interfaces
{
    /// <summary>
    /// Основной менеджер для TCP
    /// </summary>
    public interface ITcpManager
    {
        /// <summary>
        /// Словарь слушателей
        /// </summary>
        ConcurrentDictionary<Task<TcpClient>, TcpServerModel> GetTcpListeners { get; }

        /// <summary>
        /// Словарь tcp серверов
        /// </summary>
        ConcurrentDictionary<string, TcpServerModel> GetTcpServers { get; }

        /// <summary>
        /// Словарь tcp клиентов
        /// </summary>
        ConcurrentDictionary<string, TcpClientModel> GetTcpClients { get; }

        /// <summary>
        /// Перезапустить слушателей
        /// </summary>
        /// <returns></returns>
        Task RestartListeners();

        /// <summary>
        /// Запустить сервис для tcp
        /// </summary>
        /// <param name="client">Клиент</param>
        /// <param name="tcpConfiguration">Конфигурация</param>
        /// <param name="path">Путь</param>
        /// <param name="needRead">Нужно ли запустить</param>
        /// <returns></returns>
        Task<ITcpClientService> StartService(TcpClient client, TcpConfiguration tcpConfiguration, string path, bool needRead = true);
    }
}
