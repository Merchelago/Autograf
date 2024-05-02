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
    public IListFeed<Device> DevicesSearch => ListFeed.Async(async ct => await BTService.GetDevicesSearchAsync(await Search, ct), BTService.RefreshList);
    public IListFeed<string> GetHistory => ListFeed.Async(async ct => await BTService.GetHistoryAsync(ct), BTService.RefreshHistory);
    public IFeed<Device> CreatedDeletedDevice => Feed.Async(async ct => await BTService.GetCreatedDeletedDeviceAsync(ct), BTService.RefreshDeletedCreatedDevice);
    public IFeed<string> CreatedDeletedColor => Feed.Async(async ct => await BTService.GetCreatedDeletedColorDeviceAsync(ct), BTService.RefreshColorDevice);

    public IState<string> Search => State<string>.Value(this, () => "");
    public IState<int> CurrentItemsDevices => State.AsyncEnumerable(this, BTService.GetCurrentItems);
    public IState<int> AllItemsDevices => State.Async(this, BTService.GetAllItems);
    public IState<int> CurrentItemsDevicesSearch => State.AsyncEnumerable(this, BTService.GetCurrentItemsSearch);
}
