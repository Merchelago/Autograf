using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueT.Services;
using Uno.Extensions.Reactive;

namespace BlueT.Business.Models;
public partial record BtModel(IBtService BtService)
{
    public IListFeed<Device> Devices =>ListFeed.AsyncEnumerable(BtService.ScanDevicesAsync);
    public IListFeed<Device> DevicesSearch => ListFeed.Async(async ct => await BtService.GetDevicesSearchAsync(await Search, ct), BtService.RefreshList);
    public IListFeed<string> GetHistory => ListFeed.Async(async ct => await BtService.GetHistoryAsync(ct), BtService.RefreshHistory);
    public IFeed<Device> CreatedDeletedDevice => Feed.Async(async ct => await BtService.GetCreatedDeletedDeviceAsync(ct), BtService.RefreshDeletedCreatedDevice);
    public IFeed<string> CreatedDeletedColor => Feed.Async(async ct => await BtService.GetCreatedDeletedColorDeviceAsync(ct), BtService.RefreshColorDevice);

    public IState<string> Search => State<string>.Value(this, () => "");
    public IState<int> CurrentItemsDevices => State.AsyncEnumerable(this, BtService.GetCurrentItems);
    public IState<int> AllItemsDevices => State.Async(this, BtService.GetAllItems);
    public IState<int> CurrentItemsDevicesSearch => State.AsyncEnumerable(this, BtService.GetCurrentItemsSearch);
}
