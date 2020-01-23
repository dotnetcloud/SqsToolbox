using DotNetCloud.SqsToolbox.Delete;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.Configuration;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static SqsPollingQueueReaderOptions GetPollingQueueReaderOptions(this IConfiguration configuration)
        {
            var section = configuration.GetSection("SQSToolbox");

            var options = new SqsPollingQueueReaderOptions();

            var queueUrl = section.GetValue<string>("QueueUrl", null);

            if (queueUrl is object)
                options.QueueUrl = queueUrl;

            return options;
        }

        public static SqsBatchDeleterOptions GetSqsBatchDeleterOptions(this IConfiguration configuration)
        {
            var section = configuration.GetSection("SQSToolbox");

            var options = new SqsBatchDeleterOptions();

            var queueUrl = section.GetValue<string>("QueueUrl", null);

            if (queueUrl is object)
                options.QueueUrl = queueUrl;

            return options;
        }
    }
}
