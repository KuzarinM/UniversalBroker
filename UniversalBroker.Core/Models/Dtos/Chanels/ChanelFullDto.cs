﻿using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Полное Dto канала
    /// </summary>
    public class ChanelFullDto: ChanelDto
    {
        /// <summary>
        /// Выходные каналы
        /// </summary>
        public new List<ChanelDto> OutputChanels { get; set; } = new();

        /// <summary>
        /// Входные подключения
        /// </summary>
        public new List<ConnectionViewDto> InputConnections { get; set; } = new();

        /// <summary>
        /// Выходные подключения
        /// </summary>
        public new List<ConnectionViewDto> OutputConnections { get; set; } = new();
    }
}
