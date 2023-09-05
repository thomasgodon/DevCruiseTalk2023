using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SolarSystem
{
    public class SendHandlers
    {
        public SendHandlers()
        {
            
        }

        [FunctionName("SolarPanel")]
        public Task SendSolarData(
            [TimerTrigger("%SolarPanelSendInterval%")]TimerInfo timerInfo, 
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            return Task.CompletedTask;
        }

        [FunctionName("HomeBattery")]
        public Task SendBatteryData(
            [TimerTrigger("%BatterySendInterval%")] TimerInfo timerInfo, 
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            return Task.CompletedTask;
        }
    }
}
