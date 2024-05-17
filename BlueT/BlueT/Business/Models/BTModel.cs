using BlueT.Services;

namespace BlueT.Business.Models;
public partial record BtModel(IBtService BtService)
{
    public IListFeed<Device> Devices =>ListFeed.AsyncEnumerable(BtService.ScanDevicesAsync);
    public IListFeed<Device> DevicesSearch =>  ListFeed.Async(async ct => await BtService.GetDevicesSearchAsync(await Search, ct), BtService.RefreshList);
    public IListFeed<string> GetHistory => ListFeed.Async(async ct => await BtService.GetHistoryAsync(ct), BtService.RefreshHistory);
    public IFeed<Device> CreatedDeletedDevice => Feed.Async(async ct => await BtService.GetCreatedDeletedDeviceAsync(ct), BtService.RefreshDeletedCreatedDevice);
    public IState<string> Search => State.Value(this, ()=> "");
    public IState<int> CurrentItemsDevices => State.Async(this, BtService.GetCurrentItems, BtService.RefreshDeletedCreatedDevice);
    public IState<int> AllItemsDevices => State.Async(this, BtService.GetAllItems, BtService.RefreshDeletedCreatedDevice);
    public IState<int> CurrentItemsDevicesSearch => State.Async(this, BtService.GetCurrentItemsSearch, BtService.RefreshDeletedCreatedDevice);

}
