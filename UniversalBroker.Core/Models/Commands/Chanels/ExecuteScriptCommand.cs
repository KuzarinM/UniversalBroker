using MediatR;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Models.Commands.Chanels
{
    public class ExecuteScriptCommand: IRequest
    {
        public Guid ChanelId { get; set; }

        public InternalMessage Message { get; set; } = new();
    }
}
