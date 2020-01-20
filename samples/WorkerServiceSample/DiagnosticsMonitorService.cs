using DotNetCloud.SqsToolbox.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class DiagnosticsMonitorService : DiagnosticsMonitoringService
    {
        private readonly ILogger<DiagnosticsMonitorService> _logger;

        public DiagnosticsMonitorService(ILogger<DiagnosticsMonitorService> logger) => _logger = logger;

        public override void OnBegin(string queueUrl) => _logger.LogInformation("Polling for messages");

        public override void OnReceived(string queueUrl, int messageCount) => _logger.LogInformation($"Completed polling for messages. Received {messageCount}");
    }
}
