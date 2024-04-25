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
    public IListFeed<Device> Devices =>ListFeed.AsyncEnumerable(BTService.ScanDevicesAsync);

    /*public IListFeed<Device> DevicesSearch => Search
              .Where(searchTerm => searchTerm is { Length: >= 0 })
              .SelectAsync(async (searchTerm, ct) =>
              {
                  var result = await BTService.GetDevicesSearchAsync(searchTerm, ct);
                  return result;
              }).AsListFeed();*/

    public IListFeed<Device> DevicesSearch => ListFeed.Async(async ct => await BTService.GetDevicesSearchAsync(await Search.Value(ct),ct), BTService.RefreshList);
    public IState<string> Search => State<string>.Value(this, () => "");

    public IState<int> CurrentItemsDevices => State.AsyncEnumerable(this, BTService.GetCurrentItems);
    public IState<int> AllItemsDevices => State.Async(this, BTService.GetAllItems);
    public IState<int> CurrentItemsDevicesSearch => State.AsyncEnumerable(this, BTService.GetCurrentItemsSearch);

    /*public IListFeed<Device> Devices => ListFeed.Async(async (ct) => await BTService.ScanScanDevicesAsync(ct));*/
    /*public IListFeed<Device> DevicesSearch => Search
              .Where(searchTerm => searchTerm is { Length: >= 0 })
              .SelectAsync(async (searchTerm, ct) =>
              {
                      if (searchTerm == "")
                      {
                          return (await Devices);
                      }
                      else
                      {
                          // В противном случае выполнить поиск устройств по запросу
                          var result = await BTService.GetDevicesSearchAsync(searchTerm, ct).ConfigureAwait(false);
                          return result;
                      }
              })
              .AsListFeed();*/

    //public IListFeed<Device> DevicesSearch => ListFeed.AsyncEnumerable((ct) => BTService.SerchDevicesAsync(ct));


}
