namespace UniversalBroker.Core.Models.Internals
{
    public class SendingMessage
    {
        public InternalMessage Message { get; set; }

        public Guid TargerId { get; set; }

        public bool IsChanel { get; set; }
    }
}
