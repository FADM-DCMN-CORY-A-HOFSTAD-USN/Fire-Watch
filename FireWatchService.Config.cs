using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace GenetecEdwardsBridge
{
    public partial class FireWatchService
    {
        private void InitializeAndWatchConfiguration()
        {
            // Build configuration manager targeting appsettings.json with native watcher enabled
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            // Perform structural parsing sequence on initial boot up
            ParseAndValidateConfigData(isInitialBoot: true);

            // Register dynamic callback loop to handle mid-shift configuration file saves
            RegisterChangeCallback();
        }

        private void RegisterChangeCallback()
        {
            _configuration.GetReloadToken().RegisterChangeCallback(OnConfigurationFileSaved, null);
        }

        private void OnConfigurationFileSaved(object state)
        {
            // Small debounce wait window to let the plant engineer's operating system write handle clear
            Thread.Sleep(500);

            lock (_lockObject)
            {
                try
                {
                    ParseAndValidateConfigData(isInitialBoot: false);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry(ServiceName, $"Hot-swap structural pipeline exception: {ex.Message}", EventLogEntryType.Error);
                }
                finally
                {
                    // Re-arm change tracking monitoring engine thread
                    RegisterChangeCallback();
                }
            }
        }

        private void ParseAndValidateConfigData(bool isInitialBoot)
        {
            lock (_lockObject)
            {
                // Parse file strings straight into an isolated validation verification memory block
                var bufferConfig = _configuration.Get<FireWatchConfig>();
                ValidationResult summary = ConfigValidator.Validate(bufferConfig);

                if (!summary.IsValid)
                {
                    string errorsReport = string.Join(Environment.NewLine + " -> ", summary.Errors);
                    string errorMessage = $"Configuration hot-swap aborted due to schema/data errors:\n -> {errorsReport}\n\nLive service variables have not been modified.";
                    
                    if (isInitialBoot) throw new InvalidDataException(errorMessage);

                    EventLog.WriteEntry(ServiceName, errorMessage, EventLogEntryType.Error);
                    return; // ABORT SWAP: Discard the changes, preserve memory mapping configuration
                }

                if (summary.Warnings.Count > 0)
                {
                    string warningsReport = string.Join(Environment.NewLine + " -> ", summary.Warnings);
                    EventLog.WriteEntry(ServiceName, $"Configuration accepted with warnings:\n -> {warningsReport}", EventLogEntryType.Warning);
                }

                // Apply verified configurations directly into system memory targets
                _genetecConfig = bufferConfig.GenetecConfig;
                _edwardsConfig = bufferConfig.EdwardsConfig;
                _kiwiFireEventGuid = Guid.Parse(_genetecConfig.KiwiFireEventGuid);

                _lookupMap = bufferConfig.HospitalMap
                    .ToDictionary(m => m.CameraGuid.ToLower().Trim(), m => m);

                if (!isInitialBoot)
                {
                    EventLog.WriteEntry(ServiceName, $"Hot-swap succeeded. Camera routes adjusted. Active zones: {_lookupMap.Count}", EventLogEntryType.Information);
                }
            }
        }
    }
}
