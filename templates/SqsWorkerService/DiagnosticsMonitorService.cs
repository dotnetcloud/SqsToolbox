using DotNetCloud.SqsToolbox.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetCloud.SqsWorkerService
{
    public class DiagnosticsMonitorService : DiagnosticsMonitoringService
    {
        private readonly ILogger<DiagnosticsMonitorService> _logger;

        public DiagnosticsMonitorService(ILogger<DiagnosticsMonitorService> logger) => _logger = logger;

        public override void OnBegin(string queueUrl) => _logger.LogTrace("Polling for messages from {QueueUrl}.", queueUrl);

        public override void OnReceived(string queueUrl, int messageCount) => _logger.LogInformation("Received {MessageCount} messages from {QueueUrl}.", messageCount, queueUrl);
    }
}
