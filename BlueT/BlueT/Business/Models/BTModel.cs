using System.Diagnostics.Metrics;
using BlueT.Services;
using Uno.Extensions.Reactive;
using Uno.Extensions.Reactive.Sources;

namespace BlueT.Business.Models;
public partial record BtModel()
{
    readonly IBtService bt;
   
    public BtModel(IBtService BtService) :this()
    {
        bt = BtService;
        Search.ForEachAsync(async (_, ct) => new ValueTask(bt.Refresh(await Search, ct)));
        
    }

    public IListFeed<Device> Devices => ListFeed.Async(async ct => await bt.ScanDevicesAsync(ct), bt.RefreshList);
    //public IListFeed<Device> DevicesSearch => ListFeed.Async(async ct => await bt.GetDevicesSearchAsync(await Search, ct), bt.RefreshList).Where(dev => dev.DeviceName == Search.Value(CancellationToken.None).Result); 
    public IListFeed<Device> DevicesSearch => ListFeed.Async(async ct => await bt.GetDevicesSearchAsync(ct), bt.RefreshList)
        .Where(
                dev => 
                    dev.DeviceName.Contains(Search.Value(CancellationToken.None).Result,
                        StringComparison.CurrentCultureIgnoreCase)
                    ||
                    dev.DeviceType.Contains(Search.Value(CancellationToken.None).Result,
                        StringComparison.CurrentCultureIgnoreCase)
        );

    public IListFeed<string> GetHistory => ListFeed.Async(async ct => await bt.GetHistoryAsync(ct), bt.RefreshHistory);
    public IFeed<Device> CreatedDeletedDevice => Feed.Async(async ct => await bt.GetCreatedDeletedDeviceAsync(ct), bt.RefreshDeletedCreatedDevice);
    public IState<string> Search => State.Value(this, ()=> "");
    public IState<int> CurrentItemsDevices => State.Async(this, bt.GetCurrentItems, bt.RefreshDeletedCreatedDevice);
    public IState<int> AllItemsDevices => State.Async(this, bt.GetAllItems, bt.RefreshDeletedCreatedDevice);
    public IState<int> CurrentItemsDevicesSearch => State.Async(this, bt.GetCurrentItemsSearch, bt.RefreshDeletedCreatedDevice);

}
