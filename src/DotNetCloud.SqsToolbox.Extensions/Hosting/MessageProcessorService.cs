using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace DotNetCloud.SqsToolbox.Extensions.Hosting
{
    public abstract class MessageProcessorService : SqsMessageProcessingBackgroundService
    {
        protected MessageProcessorService(IChannelReaderAccessor channelReaderAccessor) : base(channelReaderAccessor)
        {
        }

        public abstract Task ProcessMessageAsync(Message message, CancellationToken cancellationToken = default);

        public sealed override async Task ProcessFromChannelAsync(ChannelReader<Message> channelReader, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP3_1
            await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
            {
                await ProcessMessageAsync(message, cancellationToken).ConfigureAwait(false);
            }
#else
            while (await channelReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                while (channelReader.TryRead(out var message))
                    await ProcessMessageAsync(message, cancellationToken).ConfigureAwait(false);
#endif
        }
    }
}
