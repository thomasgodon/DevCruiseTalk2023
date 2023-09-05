using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace TelemetryConsumer
{
    public static class EventHubHandlers
    {
        [FunctionName("TelemetryConsumer")]
        public static async Task ConsumeIotCentralTelemetry(
            [EventHubTrigger("%EventHubOptions:EventHubName%", Connection = "EventHubOptions:ConnectionString", ConsumerGroup = "%EventHubOptions:ConsumerGroup%")] EventData[] events,
            ILogger logger)
        {
            foreach (var eventData in events)
            {
                var rawEventBody = eventData.EventBody.ToString();
                logger.LogInformation(rawEventBody);
                await Task.Yield();
            }
        }
    }
}
