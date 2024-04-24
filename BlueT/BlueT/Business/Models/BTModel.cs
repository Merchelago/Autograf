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
    public IListFeed<Device> DevicesSearch => (IListFeed<Device>)Search
         .Where(search => search is { Length: > 0 })
         .Select(searchTerm =>
         {
             var result = BTService.GetDevicesSearchAsync(searchTerm, CancellationToken.None).Result;
             return result;
         }).AsListFeed();

    
    public IState<string> Search => State<string>.Value(this,()=> "");
}
