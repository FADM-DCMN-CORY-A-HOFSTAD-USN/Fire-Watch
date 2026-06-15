using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FireWatch.Network
{
    public class UnivacAegisBridge
    {
        private readonly HttpClient _httpClient;
        private readonly string _aegisEndpointUrl;

        public UnivacAegisBridge(string aegisEndpointUrl)
        {
            _aegisEndpointUrl = aegisEndpointUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5) 
            };
            
            // Standard Univac Aegis authentication headers can be added here
            _httpClient.DefaultRequestHeaders.Add("X-Aegis-Client", "FireWatch-Node-01");
        }

        public async Task<bool> PushTelemetryAsync(AegisPayload payload)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(_aegisEndpointUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AEGIS BRIDGE] Successfully routed payload: {payload.EventId}");
                    return true;
                }
                
                Console.WriteLine($"[AEGIS BRIDGE] Gateway rejected payload. Status: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AEGIS BRIDGE FATAL] Network transition failed: {ex.Message}");
                return false;
            }
        }
    }

    // Standardized payload matching the Univac Aegis expected schema
    public class AegisPayload
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string SourceNode { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public int SeverityLevel { get; set; }
        public string RawProtocolData { get; set; } = string.Empty;
    }
}
