using System.Collections.Generic;

namespace GenetecEdwardsBridge
{
    public class FireWatchConfig
    {
        public GenetecSettings GenetecConfig { get; set; }
        public EdwardsSettings EdwardsConfig { get; set; }
        public List<CameraMapping> HospitalMap { get; set; }
    }

    public class GenetecSettings
    {
        public string DirectoryServer { get; set; }
        public string ServiceUser { get; set; }
        public string ServicePassword { get; set; }
        public string KiwiFireEventGuid { get; set; }
    }

    public class EdwardsSettings
    {
        public string ReceiverIp { get; set; }
        public int ReceiverPort { get; set; }
        public int HeartbeatIntervalSeconds { get; set; }
    }

    public class CameraMapping
    {
        public string CameraGuid { get; set; }
        public string GenetecName { get; set; }
        public string EdwardsNode { get; set; }
        public string EdwardsZone { get; set; }
        public string PhysicalRoom { get; set; }
    }
}
