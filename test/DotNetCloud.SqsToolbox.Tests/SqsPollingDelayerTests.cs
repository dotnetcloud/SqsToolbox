using System;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Receive;
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
                InitialDelay = TimeSpan.FromSeconds(2) 
            });

            var result = sut.CalculateSecondsToDelay(new Message[] { new Message() });

            result.TotalSeconds.Should().Be(0);
        }

        [Fact]
        public void ReturnsInitialValue_OnFirstEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(2);
        }

        [Fact]
        public void ReturnsInitialValueToPowerOfTwo_OnSecondEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(4);
        }

        [Fact]
        public void ReturnsInitialValueToPowerOfThree_OThirdEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(8);
        }

        [Fact]
        public void ReturnsInitialValueToPowerOfFour_OFourthEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(16);
        }

        [Fact]
        public void ReturnsInitialValueToPowerOfFive_OFifthEmptyResponse()
        {
            var sut = new SqsPollingDelayer(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(32);
        }
    }
}
