namespace UniversalBroker.Adapters.Tcp.Configurations
{
    public class TcpConfiguration
    {
        public bool IsClient = true;

        public string? MessageDeviderRegex = "\n";

        public int? MessageFixSize = null;

        public byte? MessageDevicerByte = null;
    }
}
