namespace BlueT.Services;
public interface IBtService
{
    Signal RefreshList { get; }
    Signal RefreshDeletedCreatedDevice { get; }
    Signal RefreshColorDevice { get; }
    Signal RefreshHistory {  get; }
    string SearchTerm { get; set; }
    Task CreateDevicesAsync();
    Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct);
    Task<ImmutableList<string>> GetHistoryAsync(CancellationToken ct);
    Task DeleteDevicesAsync();
    IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct);
    ValueTask<int> GetAllItems(CancellationToken ct);
    ValueTask<int> GetCurrentItems(CancellationToken ct);
    ValueTask<int> GetCurrentItemsSearch(CancellationToken ct);
   
}
