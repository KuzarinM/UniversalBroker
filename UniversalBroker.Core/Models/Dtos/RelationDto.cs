using UniversalBroker.Core.Models.Enums;

namespace UniversalBroker.Core.Models.Dtos
{
    public class RelationDto
    {
        public Guid TargetId { get; set; }

        public RelationUsageStatus Status { get; set; }
    }
}
