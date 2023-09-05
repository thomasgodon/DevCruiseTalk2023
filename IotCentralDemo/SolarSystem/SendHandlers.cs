using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SolarSystem
{
    public class SendHandlers
    {
        [FunctionName("SolarPanel")]
        public async Task SendSolarData(
            [TimerTrigger("SolarPanelSendInterval")]TimerInfo _, 
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        [FunctionName("HomeBattery")]
        public async Task SendBatteryData(
            [TimerTrigger("%BatterySendInterval%")] TimerInfo _, 
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
