using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Sockets;
using System.Timers;
using Genetec.Sdk;

namespace GenetecEdwardsBridge
{
    public partial class FireWatchService
    {
        // Custom on-premise vision analytics engine initialization
        private readonly FireDetectionEngine _fireEngine = new FireDetectionEngine();
        
        // Debounce variable to track consecutive frames flagged with fire
        private int _consecutiveTriggers = 0;

        private void OnGenetecLoggedOn(object sender, LoggedOnEventArgs e)
        {
            // Subscribe natively to the live frame video streaming sequence from the Genetec Archiver
            // Note: Update the specific SDK hook name here to match the image stream callback pattern 
            // provided in your exact version of the Genetec Security Center SDK media namespace.
            _genetecEngine.MediaManager.LiveFrameReceived += OnLiveFrameReceived;
            
            EventLog.WriteEntry(ServiceName, "Successfully established authentication bridge and registered live video stream callbacks with Genetec.", EventLogEntryType.Information);
        }

        private void OnGenetecLoggedOff(object sender, LoggedOffEventArgs e)
        {
            EventLog.WriteEntry(ServiceName, "Warning: System disconnected from Genetec Directory. Re-authenticating context automatically...", EventLogEntryType.Warning);
        }

        /// <summary>
        /// Raw frame callback handler triggered directly by the Genetec SDK video feed.
        /// </summary>
        private void OnLiveFrameReceived(object sender, object mediaFrameEventArgs)
        {
            // 1. Extract raw image payload and originating camera identifier from the SDK event context
            // (Adjust property casting below to align with your specific Genetec SDK Media Event layout)
            if (mediaFrameEventArgs is ImageStreamEventArgs streamArgs)
            {
                Bitmap currentVideoFrame = streamArgs.BitmapFrame;
                string searchGuid = streamArgs.CameraGuid.ToString().ToLower().Trim();

                // 2. Stream the image frame directly into the custom mathematical color spectrum engine
                if (_fireEngine.AnalyzeFrameForFire(currentVideoFrame, out double currentDensity))
                {
                    _consecutiveTriggers++;

                    // Life-Safety Debounce: Fire spectrum signatures must persist for 5 consecutive frames
                    // to prevent studio lights, flashes, reflection shifts, or camera artifacts from tripping false alarms.
                    if (_consecutiveTriggers >= 5)
                    {
                        lock (_lockObject)
                        {
                            // Look up if the alerting camera GUID has a physical zone mapping assigned inside appsettings.json
                            if (_lookupMap.TryGetValue(searchGuid, out CameraMapping targetLocation))
                            {
                                // Frame validation passed: Transmit immediate alarm frame to Edwards FireWorks
                                SendAlertToEdwards(
                                    targetLocation.EdwardsNode, 
                                    targetLocation.EdwardsZone, 
                                    targetLocation.PhysicalRoom, 
                                    DateTime.Now
                                );
                            }
                            else
                            {
                                // Optional logging for non-monitored campus areas
                                EventLog.WriteEntry(ServiceName, $"Fire signature detected on Camera GUID '{searchGuid}', but it has no active zone layout destination defined in appsettings.json.", EventLogEntryType.Warning);
                            }
                        }
                        
                        // Reset counter post-transmission to avoid command stream saturation loops
                        _consecutiveTriggers = 0; 
                    }
                }
                else
                {
                    // Decay filter if the frame math indicates the flame signature has broken or vanished
                    if (_consecutiveTriggers > 0)
                    {
                        _consecutiveTriggers--;
                    }
                }
            }
        }

        /// <summary>
        /// Formats and atomic-transmits visual fire alarms to the Edwards FireWorks TCP ASCII driver destination.
        /// </summary>
        private void SendAlertToEdwards(string node, string zone, string physicalRoom, DateTime timestamp)
        {
            try
            {
                // Format strings into strict [STX] CSV payload [ETX][CR] bytes via encoder
                byte[] rawPacketData = EdwardsProtocolEncoder.EncodeAlarmPayload(node, zone, physicalRoom, timestamp);

                using (TcpClient client = new TcpClient())
                {
                    // Open connection with an explicit network timeout to protect the thread pool from locking up
                    var connectResult = client.ConnectAsync(_edwardsConfig.ReceiverIp, _edwardsConfig.ReceiverPort);
                    if (!connectResult.Wait(TimeSpan.FromSeconds(3)))
                    {
                        throw new TimeoutException("Network connection attempt to the Edwards host timed out after 3 seconds.");
                    }

                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.Write(rawPacketData, 0, rawPacketData.Length);
                        stream.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ServiceName, 
                    $"CRITICAL TRANSMISSION FAILURE: Could not forward video fire alert to Edwards platform at {_edwardsConfig?.ReceiverIp}:{_edwardsConfig?.ReceiverPort}.\n" +
                    $"Target Location: Node {node}, Zone {zone} ({physicalRoom}).\n" +
                    $"Exception Trace: {ex.Message}", 
                    EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Cyclic supervision watchdog handler. Fires status updates to provide platform fail-open capabilities.
        /// </summary>
        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                byte[] heartbeatPacket = EdwardsProtocolEncoder.EncodeHeartbeatPayload();

                using (TcpClient client = new TcpClient())
                {
                    var connectResult = client.ConnectAsync(_edwardsConfig.ReceiverIp, _edwardsConfig.ReceiverPort);
                    if (connectResult.Wait(TimeSpan.FromSeconds(2)))
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            stream.Write(heartbeatPacket, 0, heartbeatPacket.Length);
                            stream.Flush();
                        }
                    }
                }
            }
            catch
            {
                // Suppress network errors here to prevent cyclic error log flood storms from exhausting server I/O
            }
        }
    }
}
