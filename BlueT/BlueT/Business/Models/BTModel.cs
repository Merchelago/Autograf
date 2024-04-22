using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueT.Services;
using Uno.Extensions.Reactive;

namespace BlueT.Business.Models;
public partial record BTModel(IBTService BTService)
{
    public IListFeed<Device> Devices => ListFeed.AsyncEnumerable(BTService.ScanDevicesAsync);
    public IListFeed<Device> DevicesSearch => ListFeed.Async(async (ct) =>
    {
        var searchTerm = Search.Value(ct);
        var result = await BTService.GetDevicesSearchAsync(await searchTerm);
        return result;
    });

    /*public IListFeed<Device> DevicesSearch => Search.Where(search => search is {Length: > 0 })
        .SelectOrDefault(

        )*/
    /*public IListFeed<Device> DevicesSearch => Search
    .Where(search => search.Length > 0)
    .Select(search => BTService.GetDevicesSearchAsync(search))
    .SelectOrDefault(result => result ?? ImmutableList<Device>.Empty);
    */
    /*public IListFeed<Device> DevicesSearch => ListFeed.Async<Device>(async (ct) =>
    {
        var searchTerm = Search.Value(ct);

        if (string.IsNullOrEmpty(searchTerm))
        {
            return ImmutableList<Device>.Empty;
        }

        var result = await BTService.GetDevicesSearchAsync(searchTerm, ct);

        return result;
    });*/
    public IState<string> Search => State<string>.Value(this,()=> "");
}
