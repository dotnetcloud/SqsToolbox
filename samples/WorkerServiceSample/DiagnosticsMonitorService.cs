using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;

namespace WorkerServiceSample
{
    public class DiagnosticsMonitorService : DiagnosticsMonitoringService
    {
        private readonly ILogger<DiagnosticsMonitorService> _logger;

        public DiagnosticsMonitorService(ILogger<DiagnosticsMonitorService> logger) => _logger = logger;

        public override void OnBegin(string queueUrl) => _logger.LogInformation("Polling for messages");

        public override void OnReceived(string queueUrl, in int messageCount) => _logger.LogInformation($"Completed polling for messages. Received {messageCount}");

        public override void OnDeleteBatchCreated(in int messageCount, in long millisecondsTaken) => _logger.LogInformation($"Batch with {messageCount} message(s) created in {millisecondsTaken}ms.");

        public override void OnBatchDeleted(DeleteMessageBatchResponse deleteMessageBatchResponse, in long millisecondsTaken) => _logger.LogInformation($"Batch with {deleteMessageBatchResponse.Successful.Count} successful message(s) and {deleteMessageBatchResponse.Failed.Count} failed message(s), deleted in {millisecondsTaken}ms.");
    }
}
