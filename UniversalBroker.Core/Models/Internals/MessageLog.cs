using UniversalBroker.Core.Models.Enums;

namespace UniversalBroker.Core.Models.Internals
{
    public class MessageLog
    {
        public Guid TargetId { get; set; }
        public InternalMessage Message { get; set; } = new();
        public MessageDirection Direction { get; set; }
        public DateTime Created {  get; set; } = DateTime.UtcNow;
    }
}
