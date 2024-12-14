using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Commands.Chanels
{
    public class ChangeChanelScriptCommand: IRequest<ChanelFullDto>
    {
        public Guid Id { get; set; }

        public string ScriptText { get; set; } = string.Empty;
    }
}
