using System.Threading;
using System.Threading.Tasks;

namespace DotNetCloud.SqsToolbox.Abstractions
{
    public interface ISqsBatchDeleter : ISqsBatchDeleteQueue
    {
        void Start(CancellationToken cancellationToken = default);
        Task StopAsync();
    }
}
