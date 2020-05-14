using System.Collections.Generic;
using Amazon.SQS;
using DotNetCloud.SqsToolbox.Abstractions;
using DotNetCloud.SqsToolbox.Delete;
using DotNetCloud.SqsToolbox.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNetCloud.SqsToolbox.Extensions.Tests.DependencyInjection
{
    //public class SqsBatchDeleterServiceCollectionExtensionsTests
    //{
    //    [Fact]
    //    public void AddSqsBatchDeletion_WithConfiguration_AddsRequiredServices()
    //    {
    //        var config = new ConfigurationBuilder()
    //            .AddInMemoryCollection(new[]
    //            {
    //                new KeyValuePair<string, string>("AWS:Region", "eu-west-2"),
    //                new KeyValuePair<string, string>("SQSToolbox:QueueUrl", "https://example.com")
    //            })
    //            .Build();

    //        var services = new ServiceCollection();

    //        services.AddSingleton<IConfiguration>(config);
            
    //        services.AddSqsBatchDeletion(config);

    //        var sp = services.BuildServiceProvider();

    //        sp.GetRequiredService<IAmazonSQS>();

    //        sp.GetRequiredService<IOptions<SqsBatchDeletionOptions>>();
    //        sp.GetRequiredService<SqsBatchDeletionOptions>();

    //        var deleter = sp.GetRequiredService<ISqsBatchDeleter>();
    //        var deleterQueue = sp.GetRequiredService<ISqsBatchDeleteQueue>();

    //        deleter.Should().BeSameAs(deleterQueue);
    //    }
    //}
}
