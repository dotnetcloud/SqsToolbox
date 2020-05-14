using DotNetCloud.SqsToolbox.Abstractions;

namespace DotNetCloud.SqsToolbox.Extensions
{
    public interface ISqsPollingQueueReaderFactory
    {
        ISqsPollingQueueReader GetOrCreateReader(string name);
    }
}
