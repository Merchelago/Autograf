namespace BlueT.Services;
public interface IBtService
{
    Signal RefreshList { get; }
    Signal RefreshDeletedCreatedDevice { get; }
    Signal RefreshHistory {  get; }
    string SearchTerm { get; set; }
    Task CreateDevicesAsync(CancellationToken ct);
    Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct);
    Task<ImmutableList<string>> GetHistoryAsync(CancellationToken ct);
    Task DeleteDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesSearchAsync( CancellationToken ct);
    ValueTask<int> GetAllItems(CancellationToken ct);
    ValueTask<int> GetCurrentItems(CancellationToken ct);
    ValueTask<int> GetCurrentItemsSearch(CancellationToken ct);
    Task Refresh(string searchTerm, CancellationToken ct);
}
