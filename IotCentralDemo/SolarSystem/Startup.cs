using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using SolarSystem;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SolarSystem;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
    }
}
