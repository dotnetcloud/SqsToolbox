namespace DotNetCloud.SqsToolbox.Core.Delete
{
    //internal class SqsBatchDeleterBuilder
    //{
    //    private readonly IAmazonSQS _amazonSqs;
    //    private readonly SqsBatchDeletionOptions _sqsBatchDeletionOptions;

    //    private IExceptionHandler _exceptionHandler;
    //    private IFailedDeletionEntryHandler _failedDeletionEntryHandler;
    //    private SqsMessageChannelSource _channelSource;
    //    private Channel<Message> _channel;

    //    public SqsBatchDeleterBuilder(IAmazonSQS amazonSqs, SqsBatchDeletionOptions sqsBatchDeletionOptions)
    //    {
    //        _amazonSqs = amazonSqs ?? throw new ArgumentNullException(nameof(amazonSqs));
    //        _sqsBatchDeletionOptions = sqsBatchDeletionOptions ?? throw new ArgumentNullException(nameof(sqsBatchDeletionOptions));
    //    }

    //    public SqsBatchDeleterBuilder WithExceptionHandler(IExceptionHandler exceptionHandler)
    //    {
    //        _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));

    //        return this;
    //    }

    //    public SqsBatchDeleterBuilder WithFailedDeletionEntryHandler(IFailedDeletionEntryHandler failedDeletionEntryHandler)
    //    {
    //        _failedDeletionEntryHandler = failedDeletionEntryHandler ?? throw new ArgumentNullException(nameof(failedDeletionEntryHandler));

    //        return this;
    //    }

    //    public SqsBatchDeleterBuilder WithCustomChannel(SqsMessageChannelSource channelSource)
    //    {
    //        _channelSource = channelSource ?? throw new ArgumentNullException(nameof(channelSource));

    //        return this;
    //    }

    //    public SqsBatchDeleterBuilder WithCustomChannel(Channel<Message> channel)
    //    {
    //        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    //        return this;
    //    }

    //    public SqsBatchDeleter Build()
    //    {
    //        Channel<Message> channel;

    //        if (_channelSource is object)
    //        {
    //            channel = _channelSource.GetChannel();
    //        }
    //        else if (_channel is object)
    //        {
    //            channel = _channel;
    //        }
    //        else
    //        {
    //            channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(_sqsBatchDeletionOptions.ChannelCapacity)
    //            {
    //                SingleReader = true
    //            });
    //        }

    //        return new SqsBatchDeleter(_sqsBatchDeletionOptions, _amazonSqs, _exceptionHandler ?? DefaultExceptionHandler.Instance, _failedDeletionEntryHandler ?? DefaultFailedDeletionEntryHandler.Instance, channel);
    //    }
    //}
}
