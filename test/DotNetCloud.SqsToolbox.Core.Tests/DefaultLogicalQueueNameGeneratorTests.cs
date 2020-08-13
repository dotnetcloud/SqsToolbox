using FluentAssertions;
using Xunit;

namespace DotNetCloud.SqsToolbox.Core.Tests
{
    public class DefaultLogicalQueueNameGeneratorTests
    {
        [Fact]
        public void GenerateName_ReturnsExpectedName()
        {
            var sut = new DefaultLogicalQueueNameGenerator();

            var result = sut.GenerateName("https://sqs.eu-west-2.amazonaws.com/865288682694/TestQueue");

            result.Should().Be("eu-west-2_TestQueue");
        }

        [Fact]
        public void GenerateName_ThrowsArgumentException_WhenUrlIsNotValid()
        {
            var sut = new DefaultLogicalQueueNameGenerator();

            var result = sut.GenerateName("https://sqs.eu-west-2.amazonaws.com/865288682694/TestQueue");

            result.Should().Be("eu-west-2_TestQueue");
        }
    }
}
