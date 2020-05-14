using System;
using System.Threading.Tasks;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Delete;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetCloud.SqsToolbox.Tests.Delete
{
    //public class SqsBatchDeleterTests
    //{
    //    [Fact]
    //    public async Task Start_ThrowsWhenStartedTwice()
    //    {
    //        var options = new SqsBatchDeletionOptions
    //        {
    //            QueueUrl = "https://example.com"
    //        };

    //        var sut = new SqsBatchDeleter(options, Mock.Of<IAmazonSQS>(), Mock.Of<IExceptionHandler>(), Mock.Of<IFailedDeletionEntryHandler>());

    //        sut.Start();

    //        Action act = () => sut.Start();

    //        act.Should().Throw<InvalidOperationException>().WithMessage("The batch deleter is already started.");

    //        await sut.StopAsync();
    //    }
    //}
}
