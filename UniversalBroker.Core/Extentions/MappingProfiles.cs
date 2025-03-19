using AutoMapper;
using System.Threading.Channels;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Internals;
using Attribute = UniversalBroker.Core.Database.Models.Attribute;

namespace UniversalBroker.Core.Extentions
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            CreateMap<KeyValuePair<string, string>, Attribute>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Key, opt => opt.MapFrom(x=>x.Key))
                .ForMember(x=>x.Value, opt => opt.MapFrom(x=>x.Value));

            AddCommunicationsMappings();

            AddConnectionsMappings();

            AssChannelsMappings();

            AddLoggsMappings();

            AddMessagesMappings();
        }

        private void AddCommunicationsMappings()
        {
            CreateMap<Communication, CommunicationDto>()
                .ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.CommunicationAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)));

            CreateMap<CreateCommunicationDto, Communication>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Status, opt => opt.MapFrom(x => true));

            CreateMap<KeyValuePair<string, string>, CommunicationAttribute>()
               .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
               .ForMember(x => x.Attribute, opt => opt.MapFrom(x => x));

            CreateMap<Protos.CommunicationDto, CreateCommunicationDto>();
            CreateMap<Protos.CommunicationFullDto, CreateCommunicationDto>();

            CreateMap<CommunicationDto, Protos.CommunicationFullDto>()
                .ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.Attributes.Select(x => new Protos.AttributeDto()
                {
                    Name = x.Key,
                    Value = x.Value,
                })));

            CreateMap<Protos.CommunicationFullDto, CommunicationSetAttributeCommand>()
                .ForMember(x => x.CommunicationId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.Attributes.ToDictionary(x => x.Name, x => x.Value))); //todo тут не предусмотрены отправка повторов
        }

        private void AddConnectionsMappings()
        {
            CreateMap<KeyValuePair<string, string>, ConnectionAttribute>()
               .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
               .ForMember(x => x.Attribute, opt => opt.MapFrom(x => x));

            CreateMap<KeyValuePair<string, string>, Protos.AttributeDto>()
              .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Key))
              .ForMember(x => x.Value, opt => opt.MapFrom(x => x.Value));

            CreateMap<CreateConnectionDto, Connection>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.ConnectionAttributes, opt => opt.MapFrom(x => x.Attribues.ToList()));

            CreateMap<Connection, ConnectionDto>()
                .ForMember(x => x.Attribues, opt => opt.MapFrom(x => x.ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)));


            CreateMap<Connection, Protos.ConnectionDeleteDto>();

            CreateMap<Connection, ConnectionFullDto>()
                .ForMember(x => x.Attribues, opt => opt.MapFrom(x => x.ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)))
                .ForMember(x=>x.ChannelsIds, opt => opt.MapFrom(x=>x.Chanels.Select(x=>x.Id).ToList()));

            CreateMap<Connection, Protos.ConnectionDto>()
               .ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.ConnectionAttributes.ToDictionary(x => x.Attribute.Key, x => x.Attribute.Value)));

            CreateMap<ConnectionDto, Protos.ConnectionDto>()
                .ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.Attribues.Select(x => new Protos.AttributeDto()
                {
                    Name = x.Key,
                    Value = x.Value,
                })));

            CreateMap<Protos.ConnectionDto, UpdateConnectionDto>()
               .ForMember(x => x.Attribues, opt => opt.MapFrom(x => x.Attributes.ToDictionary(x => x.Name, x => x.Value)));
        }

        private void AssChannelsMappings()
        {
            CreateMap<Chanel, ChanelDto>()
                .ForMember(x => x.Script, opt => opt.MapFrom(x => x.Script.Path))//TODO Временная мера. Потом пределаю на нормально
                .ForMember(x => x.InputConnections, opt => opt.MapFrom(x => x.Connections.Where(x => x.Isinput).Select(x => x.Id)))
                .ForMember(x => x.OutputConnections, opt => opt.MapFrom(x => x.Connections.Where(x => !x.Isinput).Select(x => x.Id)))
                .ForMember(x => x.OutputChanels, opt => opt.MapFrom(x => x.FromChanels.Select(x => x.Id)));

            CreateMap<Chanel, ChanelFullDto>()
                .ForMember(x => x.Script, opt => opt.MapFrom(x => x.Script.Path))//TODO Временная мера. Потом пределаю на нормально
                .ForMember(x => x.InputConnections, opt => opt.MapFrom(x => x.Connections.Where(x => x.Isinput)))
                .ForMember(x => x.OutputConnections, opt => opt.MapFrom(x => x.Connections.Where(x => !x.Isinput)))
                .ForMember(x => x.OutputChanels, opt => opt.MapFrom(x => x.FromChanels));

            CreateMap<CreateChanelDto, Chanel>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Script, opt => opt.MapFrom(x => new Script() { Id = Guid.NewGuid(), Path = x.Script }));
        }

        private void AddLoggsMappings()
        {
            CreateMap<ScriptExecutionLog, ExecutionLog>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Datetime, opt => opt.MapFrom(x => x.Created))
                .ForMember(x => x.Lavel, opt => opt.MapFrom(x => x.LogLevel.ToString()))
                .ForMember(x => x.Text, opt => opt.MapFrom(x => x.MessageText));

            CreateMap<ExecutionLog, ChanelScriptLogDto>();
        }

        private void AddMessagesMappings()
        {
            CreateMap<KeyValuePair<string, string>, Header>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Key))
                .ForMember(x => x.Value, opt => opt.MapFrom(x => x.Value));

            CreateMap<MessageLog, Message>()
                .ForMember(x => x.Id, opt => opt.MapFrom(x => Guid.NewGuid()))
                .ForMember(x => x.Datetime, opt => opt.MapFrom(x => x.Created))
                .ForMember(x => x.Data, opt => opt.MapFrom(x => x.Message.Data))
                .ForMember(x => x.ConnectionId, opt => opt.MapFrom(x => GetConnectionIdForMessage(x)))
                .ForMember(x => x.SourceChannelId, opt => opt.MapFrom(x => GetSourceChannelIdForMessage(x)))
                .ForMember(x => x.TargetChannelId, opt => opt.MapFrom(x => GetTargetChannelIdForMessage(x)))
                .ForMember(x => x.Headers, opt => opt.MapFrom(x => x.Message.Headers.ToList()));

            CreateMap<Message, MessageViewDto>()
                .ForMember(x => x.Direction, opt => opt.MapFrom(x => GetDirectionByMessage(x)))
                .ForMember(x => x.SourceId, opt => opt.MapFrom(x => x.Connection != null && x.Connection.Isinput ? x.ConnectionId : x.SourceChannelId))
                .ForMember(x => x.SourceName, opt => opt.MapFrom(x => 
                                                                    x.Connection != null && x.Connection.Isinput ? 
                                                                        (x.Connection != null ? x.Connection.Name : string.Empty) : 
                                                                        (x.SourceChannel != null ? x.SourceChannel.Name : string.Empty)
                                                                ))
                .ForMember(x => x.TargetId, opt => opt.MapFrom(x => x.Connection != null && !x.Connection.Isinput ? x.ConnectionId : x.TargetChannelId))
                .ForMember(x => x.TargetName, opt => opt.MapFrom(x => 
                                                                    x.Connection != null && !x.Connection.Isinput ? 
                                                                    (x.Connection != null ? x.Connection.Name : string.Empty) :
                                                                    (x.TargetChannel != null ? x.TargetChannel.Name : string.Empty)
                                                                ))
                .ForMember(x => x.Headers, opt => opt.MapFrom(x => x.Headers.ToDictionary(y => y.Name, y => y.Value)));
        }

        public Guid? GetConnectionIdForMessage(MessageLog message) =>
            message.Direction switch
            {
                MessageDirection.ChanelToChanel => null,
                MessageDirection.ChanelToConnection => message.TargetId,
                MessageDirection.ConnectionToChanel => message.Message.SourceId,
                _ => null
            };
        public Guid? GetSourceChannelIdForMessage(MessageLog message) =>
            message.Direction switch
            {
                MessageDirection.ChanelToChanel => message.Message.SourceId,
                MessageDirection.ChanelToConnection => message.Message.SourceId,
                MessageDirection.ConnectionToChanel => null,
                _ => null
            };
        public Guid? GetTargetChannelIdForMessage(MessageLog message) =>
            message.Direction switch
            {
                MessageDirection.ChanelToChanel => message.TargetId,
                MessageDirection.ChanelToConnection => null,
                MessageDirection.ConnectionToChanel => message.TargetId,
                _ => null
            };
        public MessageDirection GetDirectionByMessage(Message message)
        {
            if (message.Connection != null)
                return message.Connection.Isinput ? MessageDirection.ConnectionToChanel : MessageDirection.ChanelToConnection;
            return MessageDirection.ChanelToChanel;
        }
    }
}
