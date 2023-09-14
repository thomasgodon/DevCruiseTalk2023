using SolarSystem.Telemetry;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SolarSystem
{
    internal class Program
    {
        private const string SolarPanelDeviceId = "";
        private const string SolarPanelIdScope = "";
        private const string SolarPanelPrimaryKey = "";
        private const string SolarPanelSecondaryKey = "";

        private const string HomeBatteryDeviceId = "";
        private const string HomeBatteryIdScope = "";
        private const string HomeBatteryPrimaryKey = "";
        private const string HomeBatterySecondaryKey = "";

        private const string ProvisioningUri = "global.azure-devices-provisioning.net";

        private static async Task Main()
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                Console.WriteLine("Canceling...");
                cts.Cancel();
                e.Cancel = true;
            };

            // create solar panel device & register
            var solarBatteryOptions = new DeviceOptions()
            {
                DeviceId = SolarPanelDeviceId,
                IdScope = SolarPanelIdScope,
                PrimaryKey = SolarPanelPrimaryKey,
                SecondaryKey = SolarPanelSecondaryKey,
            };
            var solarPanelDevice = await CreateDeviceClientAsync(solarBatteryOptions, cts.Token);
            if (solarPanelDevice == null)
            {
                Console.ReadLine();
                return;
            }

            // create home battery device & register
            var homeBatteryOptions = new DeviceOptions()
            {
                DeviceId = HomeBatteryDeviceId,
                IdScope = HomeBatteryIdScope,
                PrimaryKey = HomeBatteryPrimaryKey,
                SecondaryKey = HomeBatterySecondaryKey,
            };
            var homeBatteryDevice = await CreateDeviceClientAsync(homeBatteryOptions, cts.Token);
            if (homeBatteryDevice == null)
            {
                Console.ReadLine();
                return;
            }

            while (cts.Token.IsCancellationRequested is false)
            {
                await SendSolarPanelTelemetryAsync(solarPanelDevice, cts.Token);
                await SendHomeBatteryTelemetryAsync(homeBatteryDevice, cts.Token);

                // wait 500ms to send new data
                new ManualResetEvent(false).WaitOne(500);
            }
        }

        private static async Task SendSolarPanelTelemetryAsync(DeviceClient iotDevice, CancellationToken cancellationToken)
        {
            var telemetry = CreateSolarTelemetry();
            var message = CreateIotDeviceMessage(telemetry);
            await iotDevice.SendEventAsync(message, cancellationToken);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry, Formatting.Indented));
        }

        private static async Task SendHomeBatteryTelemetryAsync(DeviceClient iotDevice, CancellationToken cancellationToken)
        {
            var telemetry = CreateHomeBatterTelemetry();
            var message = CreateIotDeviceMessage(telemetry);
            await iotDevice.SendEventAsync(message, cancellationToken);
            Console.WriteLine(JsonConvert.SerializeObject(telemetry, Formatting.Indented));
        }

        private static SolarPanelTelemetry CreateSolarTelemetry() =>
            new()
            {
                Id = 1,
                Power = new Random().NextDouble() * 3000,
                Voltage = new Random().NextDouble() * 230
            };

        private static HomeBatteryTelemetry CreateHomeBatterTelemetry() =>
            new()
            {
                Id = 2,
                Poc = new Random().NextDouble() * 100,
            };

        private static Message CreateIotDeviceMessage<T>(T telemetry) where T : ITelemetry
        {
            var serializedTelemetry = JsonSerializer.Serialize(telemetry);
            return new Message(Encoding.UTF8.GetBytes(serializedTelemetry));
        }

        private static async Task<DeviceClient?> CreateDeviceClientAsync(DeviceOptions deviceOptions, CancellationToken cancellationToken)
        {
            var underlyingIotHub = await GetUnderlyingIotHub(deviceOptions, cancellationToken);

            if (underlyingIotHub == null)
            {
                return null;
            }

            var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(deviceOptions.DeviceId, deviceOptions.PrimaryKey);
            var client = DeviceClient.Create(underlyingIotHub, authMethod, TransportType.Amqp);
            if (client == null)
            {
                Console.WriteLine($"Could not create device {deviceOptions.DeviceId}");
            }

            Console.WriteLine($"Device '{deviceOptions.DeviceId}' registered successfully.");

            return client;
        }

        private static async Task<string?> GetUnderlyingIotHub(DeviceOptions deviceOptions, CancellationToken cancellationToken)
        {
            try
            {
                using var symmetricKeyProvider = new SecurityProviderSymmetricKey(deviceOptions.DeviceId, deviceOptions.PrimaryKey, deviceOptions.SecondaryKey);
                var dps = ProvisioningDeviceClient.Create(ProvisioningUri, deviceOptions.IdScope, symmetricKeyProvider, new ProvisioningTransportHandlerAmqp());
                var registerResult = await dps.RegisterAsync(cancellationToken);
                return registerResult.AssignedHub;
            }
            catch (Exception)
            {
                Console.WriteLine($"Could not get underlying iot hub for device '{deviceOptions.DeviceId}'");
                return null;
            }
        }
    }
}