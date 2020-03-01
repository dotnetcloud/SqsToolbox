using System;
using Amazon.SQS.Model;
using DotNetCloud.SqsToolbox.Receive;
using FluentAssertions;
using Xunit;

namespace DotNetCloud.SqsToolbox.Tests
{
    public class SqsPollingDelayerTests
    {
        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 4)]
        [InlineData(3, 8)]
        [InlineData(4, 16)]
        [InlineData(5, 32)]
        public void ReturnsExpectedDelay_WhenUsingExponentialBackOff(int numberOfEmptyPolls, int expected)
        {
            var sut = new SqsReceivePollDelayCalculator(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(2)
            });

            TimeSpan result = TimeSpan.FromSeconds(-1);

            for (int i = 0; i < numberOfEmptyPolls; i++)
            {
                result = sut.CalculateSecondsToDelay(Array.Empty<Message>());
            }

            result.TotalSeconds.Should().Be(expected);
        }

        [Fact]
        public void ReturnsZeroTimeSpan_WhenMoreThanOneMessage()
        {
            var sut = new SqsReceivePollDelayCalculator(new SqsPollingQueueReaderOptions 
            { 
                InitialDelay = TimeSpan.FromSeconds(2) 
            });

            var result = sut.CalculateSecondsToDelay(new Message[] { new Message() });

            result.TotalSeconds.Should().Be(0);
        }       

        [Fact]
        public void ReturnsMaxValue_WhenSet()
        {
            var sut = new SqsReceivePollDelayCalculator(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(10),
                MaxDelay = TimeSpan.FromSeconds(5)
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());
           
            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(5);
        }

        [Fact]
        public void ReturnsInitialDelayForAllCalls_WhenNotUsingExponentialBackOff()
        {
            var sut = new SqsReceivePollDelayCalculator(new SqsPollingQueueReaderOptions
            {
                InitialDelay = TimeSpan.FromSeconds(10),
                UseExponentialBackoff = false
            });

            sut.CalculateSecondsToDelay(Array.Empty<Message>());
            sut.CalculateSecondsToDelay(Array.Empty<Message>());

            var result = sut.CalculateSecondsToDelay(Array.Empty<Message>());

            result.TotalSeconds.Should().Be(10);
        }
    }
}
