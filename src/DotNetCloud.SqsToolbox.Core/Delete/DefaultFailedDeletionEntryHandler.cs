﻿using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Core.Abstractions;

namespace DotNetCloud.SqsToolbox.Core.Delete
{
    internal sealed class DefaultFailedDeletionEntryHandler : IFailedDeletionEntryHandler
    {
        public static DefaultFailedDeletionEntryHandler Instance = new DefaultFailedDeletionEntryHandler();

        public Task OnFailureAsync(BatchResultErrorEntry batchResultErrorEntry, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
