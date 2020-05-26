using System.Threading;
using System.Threading.Tasks;

namespace DotNetCloud.SqsToolbox.Core.Abstractions
{
    internal interface ISqsBatchDeleter : ISqsBatchDeleteQueue
    {
        void Start(CancellationToken cancellationToken = default);
        Task StopAsync();
    }
}
