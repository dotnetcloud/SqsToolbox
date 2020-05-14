using System;
using DotNetCloud.SqsToolbox.Receive;
using Microsoft.Extensions.Configuration;

namespace DotNetCloud.SqsToolbox.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static SqsPollingQueueReaderOptions GetPollingQueueReaderOptions(this IConfiguration configuration)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            var section = configuration.GetSection("SQSToolbox");

            var options = new SqsPollingQueueReaderOptions
            {
                QueueUrl = section.GetValue<string>("QueueUrl", null)
            };

            return options;
        }

        //public static SqsBatchDeletionOptions GetSqsBatchDeleterOptions(this IConfiguration configuration)
        //{
        //    _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

        //    var section = configuration.GetSection("SQSToolbox");

        //    var options = new SqsBatchDeletionOptions
        //    {
        //        QueueUrl = section.GetValue<string>("QueueUrl", null)
        //    };

        //    return options;
        //}
    }
}
