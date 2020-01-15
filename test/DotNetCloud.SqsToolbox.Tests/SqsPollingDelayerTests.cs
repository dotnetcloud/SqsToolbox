using System;
using Amazon.SQS.Model;
using FluentAssertions;
using Xunit;

namespace DotNetCloud.SqsToolbox.Tests
{
    public class SqsPollingDelayerTests
    {
        [Fact]
        public void ReturnsZeroTimeSpan_WhenMoreThanOneMessage()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions 
            { 
                InitialDelayWhenEmpty = TimeSpan.FromSeconds(1) 
            });

            var result = sut.CalculateSecondsToDelay(new Message[] { new Message() });

            result.TotalSeconds.Should().Be(0);
        }

        [Fact]
        public void ReturnsInitialValue_OnFirstEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelayWhenEmpty = TimeSpan.FromSeconds(1)
            });

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(1);
        }

        [Fact]
        public void ReturnsInitialValueToPowerOfTwo_OnSecondEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelayWhenEmpty = TimeSpan.FromSeconds(1)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(2);
        }
    }
}
