using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolarSystem.Settings;
using SolarSystem.Telemetry;

namespace SolarSystem
{
    public class SendHandlers
    {
        private readonly DeviceSettingsOptions _deviceSettingsOptions;
        private readonly Random _random = new();
        private const string ProvisioningUri = "global.azure-devices-provisioning.net";

        public SendHandlers(IOptions<DeviceSettingsOptions> deviceSettingsOptions)
        {
            _deviceSettingsOptions = deviceSettingsOptions.Value;
        }

        [FunctionName("SolarPanel")]
        public async Task SendSolarData(
            [TimerTrigger("%SolarPanelSendInterval%")]TimerInfo timerInfo, 
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var telemetry = CreateSolarTelemetry();
            var iotDevice = await CreateDeviceClientAsync(_deviceSettingsOptions.SolarPanel, logger, cancellationToken);
            if (iotDevice == null)
            {
                return;
            }
            var message = CreateIotDeviceMessage(telemetry);
            await iotDevice.SendEventAsync(message, cancellationToken);
        }

        [FunctionName("HomeBattery")]
        public async Task SendBatteryData(
            [TimerTrigger("%BatterySendInterval%")] TimerInfo timerInfo, 
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var telemetry = CreateHomeBatterTelemetry();
            var iotDevice = await CreateDeviceClientAsync(_deviceSettingsOptions.HomeBattery, logger, cancellationToken);
            if (iotDevice == null)
            {
                return;
            }
            var message = CreateIotDeviceMessage(telemetry);
            await iotDevice.SendEventAsync(message, cancellationToken);
        }

        private SolarTelemetry CreateSolarTelemetry() =>
            new()
            {
                Id = 1,
                Power = _random.NextDouble() * 3000,
                Voltage = _random.NextDouble() * 230
            };

        private HomeBatteryTelemetry CreateHomeBatterTelemetry() =>
            new()
            {
                Id = 1,
                Poc = _random.NextDouble() * 100,
            };

        private static Message CreateIotDeviceMessage<T>(T telemetry) where T : ITelemetry
        {
            var serializedTelemetry = JsonSerializer.Serialize(telemetry);
            return new Message(Encoding.UTF8.GetBytes(serializedTelemetry));
        }

        private static async Task<DeviceClient> CreateDeviceClientAsync(DeviceOptions deviceOptions, ILogger logger, CancellationToken cancellationToken)
        {
            var underlyingIotHub = await GetUnderlyingIotHub(deviceOptions, logger, cancellationToken);

            if (underlyingIotHub == null)
            {
                return null;
            }

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceOptions.DeviceId, deviceOptions.PrimaryKey);
            var client = DeviceClient.Create(underlyingIotHub, authMethod, TransportType.Amqp);
            if (client == null)
            {
                logger.LogError("Could not create device {deviceId}", deviceOptions.DeviceId);
            }

            return client;
        }

        private static async Task<string> GetUnderlyingIotHub(DeviceOptions deviceOptions, ILogger logger, CancellationToken cancellationToken)
        {
            try
            {
                using var symmetricKeyProvider = new SecurityProviderSymmetricKey(deviceOptions.DeviceId, deviceOptions.PrimaryKey, deviceOptions.SecondaryKey);
                var dps = ProvisioningDeviceClient.Create(ProvisioningUri, deviceOptions.IdScope, symmetricKeyProvider, new ProvisioningTransportHandlerAmqp());
                var registerResult = await dps.RegisterAsync(cancellationToken);
                return registerResult.AssignedHub;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not get underlying iot hub");
                return null;
            }
        }
    }
}
