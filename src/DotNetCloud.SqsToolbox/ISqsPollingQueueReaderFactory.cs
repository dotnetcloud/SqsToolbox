using DotNetCloud.SqsToolbox.Core.Abstractions;

namespace DotNetCloud.SqsToolbox
{
    public interface ISqsPollingQueueReaderFactory
    {
        ISqsPollingQueueReader GetOrCreateReader(string name);
    }
}
