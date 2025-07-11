﻿using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Dtos.Connections
{
    /// <summary>
    /// Полное Dto подключения
    /// </summary>
    public class ConnectionFullDto: ConnectionViewDto
    {
        /// <summary>
        /// Модель подключения
        /// </summary>
        public CommunicationDto Communication { get; set; } = new();

        public List<Guid> ChannelsIds { get; set; } = new();
    }
}
