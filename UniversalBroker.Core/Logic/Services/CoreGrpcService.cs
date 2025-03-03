using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using MediatR;
using Protos;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Communications;
using static Protos.CoreService;

namespace UniversalBroker.Core.Logic.Services
{
    public class CoreGrpcService(
        ILogger<CoreGrpcService> logger,
        IMediator mediator,
        IMapper mapper) : CoreServiceBase
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IMapper _mapper = mapper;

        public override async Task<CommunicationFullDto> Init(Protos.CommunicationDto request, ServerCallContext context)
        {
            var communicationCreateDto = _mapper.Map<CreateCommunicationDto>(request);

            var resInternalDto = await _mediator.Send(new AddOrUpdateCommunicationCommand()
            {
                CreateCommunicationDto = communicationCreateDto,
            });

            var res = _mapper.Map<CommunicationFullDto>(resInternalDto);

            return res;
        }

        public override Task Connect(IAsyncStreamReader<CoreMessage> requestStream, IServerStreamWriter<CoreMessage> responseStream, ServerCallContext context)
        {
            return base.Connect(requestStream, responseStream, context);
        }

        public override Task<ConnectionsList> LoadInConnections(CommunicationSmallDto request, ServerCallContext context)
        {
            return base.LoadInConnections(request, context);
        }

        public override Task<ConnectionsList> LoadOutConnections(CommunicationSmallDto request, ServerCallContext context)
        {
            return base.LoadOutConnections(request, context);
        }

        public override Task<Empty> Disconnect(CommunicationSmallDto request, ServerCallContext context)
        {
            return base.Disconnect(request, context);
        }
    }
}
