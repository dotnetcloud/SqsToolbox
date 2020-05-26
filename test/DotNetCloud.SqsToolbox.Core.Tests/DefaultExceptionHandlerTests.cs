using FluentAssertions;
using Xunit;

namespace DotNetCloud.SqsToolbox.Core.Tests
{
    public class DefaultExceptionHandlerTests
    {
        [Fact]
        public void Instance_ReturnsSameInstanceEachTime()
        {
            var instance1 = DefaultExceptionHandler.Instance;
            var instance2 = DefaultExceptionHandler.Instance;

            instance1.Should().BeSameAs(instance2);
        }
    }
}
