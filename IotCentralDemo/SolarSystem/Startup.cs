using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarSystem;
using SolarSystem.Settings;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SolarSystem;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddOptions<DeviceSettingsOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(DeviceSettingsOptions.Name).Bind(options);
            });
    }
}
