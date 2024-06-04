using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extensions.Reactive.Testing;

namespace BlueT.Tests;
[TestClass]
public class BtModelTest: FeedTests
{
    /*
    [TestMethod]
    public async Task When_ProviderReturnsValueSync_Then_GetSome()
    {
        var sut = Feed.Async(async ct =>
        {

        });
    }*/
    [Test]
    public async Task When_ProviderReturnsValueSync_Then_GetSome()
    {
        var sut = Feed.Async(async ct =>
        {
            await Task.Delay(1, ct);
            return 42;
        });
        using var result = sut.Record();

        result.Should().Be(r => r
        
            .Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
            .Message(Changed.Data, 42, Error.No, Progress.Final)
        );
    }
}
