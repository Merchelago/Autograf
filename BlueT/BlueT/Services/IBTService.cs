using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueT.Services;
public interface IBTService
{
    Signal RefreshList { get; }
    Signal RefreshDeletedCreatedDevice { get; }
    Signal RefreshColorDevice { get; }
    Signal RefreshHistory {  get; }
    Task CreateDevicesAsync();
    Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct);
    Task<string> GetCreatedDeletedColorDeviceAsync(CancellationToken ct);
    Task<ImmutableList<string>> GetHistoryAsync(CancellationToken ct);
    Task DeleteDevicesAsync();
    IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct);
    ValueTask<int> GetAllItems(CancellationToken ct);
    IAsyncEnumerable<int> GetCurrentItems(CancellationToken ct);
    IAsyncEnumerable<int> GetCurrentItemsSearch(CancellationToken ct);
}
