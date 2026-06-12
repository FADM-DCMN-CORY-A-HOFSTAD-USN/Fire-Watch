using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Net.Sockets;
using System.Timers;
using Genetec.Sdk;
using Genetec.Sdk.Events;

namespace GenetecEdwardsBridge
{
    public partial class FireWatchService : ServiceBase
    {
        private Engine m_engine;
        private Timer m_heartbeatTimer;
        
        // Configuration Parameters - Adjust to match your hospital environment
        private const string GenetecServer = "10.100.20.5"; 
        private const string GenetecUser = "EdwardsBridgeUser";
        private const string GenetecPass = "SecurePassword123";
        private const string EdwardsIp = "10.100.50.12";
        private const int EdwardsPort = 2323; // Target port for Edwards Text/ASCII Driver
        
        // Replace with your system's specific KiwiVision Fire Detection GUID
        private static readonly Guid KiwiFireEventGuid = new Guid("00000000-0000-0000-0000-000000000000"); 

        public FireWatchService()
        {
            ServiceName = "GenetecEdwardsFireWatch";
        }

        protected override void OnStart(string[] args)
        {
            m_engine = new Engine();
            m_engine.LoginManager.LoggedOn += OnGenetecLoggedOn;
            m_engine.LoginManager.LoggedOff += OnGenetecLoggedOff;
            
            // Begin Asynchronous Login to Genetec Directory
            m_engine.LoginManager.LogonAsync(GenetecServer, GenetecUser, GenetecPass);

            // Initialize 60-second Supervision Heartbeat
            m_heartbeatTimer = new Timer(60000);
            m_heartbeatTimer.Elapsed += SendHeartbeatToEdwards;
            m_heartbeatTimer.Start();
        }

        private void OnGenetecLoggedOn(object sender, LoggedOnEventArgs e)
        {
            // Subscribe natively to system-wide Video Analytics Events
            m_engine.ActionManager.RegisterAction(EventType.VideoAnalyticsEvent, OnAnalyticsEventReceived);
        }

        private void OnAnalyticsEventReceived(object sender, ActionReceivedEventArgs e)
        {
            if (e.Event is VideoAnalyticsEvent analyticsEvent)
            {
                // Verify if the event matches the KiwiVision Fire pattern
                if (analyticsEvent.AnalyticsTypeGuid == KiwiFireEventGuid)
                {
                    string cameraName = analyticsEvent.SourceName;
                    Guid cameraGuid = analyticsEvent.SourceGuid;
                    DateTime eventTime = analyticsEvent.Timestamp;

                    SendAlertToEdwards(cameraName, cameraGuid, eventTime);
                }
            }
        }

        private void SendAlertToEdwards(string camName, Guid camGuid, DateTime timestamp)
        {
            try
            {
                using (TcpClient client = new TcpClient(EdwardsIp, EdwardsPort))
                using (NetworkStream stream = client.GetStream())
                {
                    // Construct a standardized ASCII string mapping the camera to the Edwards node zone
                    // Format: \x02[ALARM]|[SOURCE]|[GUID]|[TIMESTAMP]\x03
                    string rawPayload = $"ALARM|FIRE|CAM:{camName}|ID:{camGuid:D}|TIME:{timestamp:O}";
                    byte[] payload = Encoding.ASCII.GetBytes($"\x02{rawPayload}\x03");
                    
                    stream.Write(payload, 0, payload.Length);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("GenetecEdwardsBridge", $"Failed sending alarm packet: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void SendHeartbeatToEdwards(object sender, ElapsedEventArgs e)
        {
            try
            {
                using (TcpClient client = new TcpClient(EdwardsIp, EdwardsPort))
                using (NetworkStream stream = client.GetStream())
                {
                    // Supervision Heartbeat ensures Edwards triggers a fault if the plugin goes offline
                    byte[] payload = Encoding.ASCII.GetBytes("\x02STATUS|HEARTBEAT|BRIDGE:OK\x03");
                    stream.Write(payload, 0, payload.Length);
                }
            }
            catch 
            {
                // Suppress failure here to prevent heartbeat loop from crashing the Windows Service
            }
        }

        protected override void OnStop()
        {
            m_heartbeatTimer.Stop();
            if (m_engine.LoginManager.IsLoggedOn)
            {
                m_engine.ActionManager.UnregisterAction(EventType.VideoAnalyticsEvent, OnAnalyticsEventReceived);
                m_engine.LoginManager.Logoff();
            }
            m_engine.Dispose();
        }
    }
}
