using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using MediatR;
using Protos;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Queries.Connections;
using static Google.Rpc.Context.AttributeContext.Types;
using static Protos.CoreService;

namespace UniversalBroker.Core.Logic.Services
{
    public class CoreGrpcService(
        ILogger<CoreGrpcService> logger,
        IMediator mediator,
        IMapper mapper,
        AbstractAdaptersManager adaptersManager) : CoreServiceBase
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IMapper _mapper = mapper;
        private readonly AbstractAdaptersManager _adaptersManager = adaptersManager;

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
            var adapterCoreService = _adaptersManager.CreateService;

            adapterCoreService.StartWork(requestStream, responseStream);

            return Task.CompletedTask;
        }

        public override async Task<ConnectionsList> LoadInConnections(CommunicationSmallDto request, ServerCallContext context)
        {
            var communicationId = Guid.TryParse(request.Id, out var communication)? communication : (Guid?)null;

            var rawRes = await _mediator.Send(new GetConnectionListQuery()
            {
                PageSize = 100000,
                PageNumber = 0,
                InputOnly = true,
                CommunicationId = communicationId
            });

            return new ConnectionsList
            {
                Connections = { _mapper.Map<List<ConnectionDto>>(rawRes) }
            };
        }

        public override async Task<ConnectionsList> LoadOutConnections(CommunicationSmallDto request, ServerCallContext context)
        {
            var communicationId = Guid.TryParse(request.Id, out var communication) ? communication : (Guid?)null;

            var rawRes = await _mediator.Send(new GetConnectionListQuery()
            {
                PageSize = 100000,
                PageNumber = 0,
                InputOnly = false,
                CommunicationId = communicationId
            });

            return new ConnectionsList
            {
                Connections = { _mapper.Map<List<ConnectionDto>>(rawRes) }
            };
        }

        public override async Task<Empty> Disconnect(CommunicationSmallDto request, ServerCallContext context)
        {
            var communicationId = Guid.TryParse(request.Id, out var communication) ? communication : (Guid?)null;

            await _adaptersManager.GetAdapterById(communicationId!.Value)!.Stop();

            return new();
        }
    }
}
