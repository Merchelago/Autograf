using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Extensions.Reactive.Testing;
using BlueT.Services;
using System.Collections.Immutable;
namespace BlueT.Tests;
[TestClass]
public class BtModelTest: FeedTests
{
    BtService bt = new BtService();
   
    [Test]
    public async Task When_ProviderReturnsValueSync_Then_GetSome()
    {
        var sut = ListFeed.Async(async ct => await bt.GetDevicesAsync(ct), bt.RefreshList);
        using var result = sut.Record();
        await Task.Delay(2100);
        await result.Should().BeAsync(r => r
            .Message(Changed.Data, Data.None, Error.No, Progress.Final)
            //.Message(Changed.Progress, Data.Undefined, Error.No, Progress.Transient)
            .Message(Changed.Data & Changed.Refreshed, Data.Some , Error.No, Progress.Final)
        );
    }
}
