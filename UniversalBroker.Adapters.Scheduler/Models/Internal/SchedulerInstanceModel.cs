using Protos;
using System.Text;
using UniversalBroker.Adapters.Scheduler.Configurations;
using Timer = System.Timers.Timer;

namespace UniversalBroker.Adapters.Scheduler.Models.Internal
{
    public class SchedulerInstanceModel
    {
        public Timer? MyTimer { get; set; }

        public SchedulerConfiguration SchedulerConfiguration { get; set; }

        public byte[] MessageBody { get; set; } = Encoding.UTF8.GetBytes("Сообщение от планировщика");

        public CancellationTokenSource CancellationTokenSource { get; set; } = new();

        public ConnectionDto Connection { get; set; }
    }
}
