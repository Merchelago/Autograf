using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueT.Services;
public interface IBTService
{
    string SearchName { get; set; }
    Task CreateDevicesAsync();
    Task DeleteDevicesAsync();
    IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct);

    IAsyncEnumerable<ImmutableList<Device>> SerchDevicesAsync(CancellationToken ct);

    ValueTask<int> GetAllItems(CancellationToken ct);
    IAsyncEnumerable<int> GetCurrentItems(CancellationToken ct);
    IAsyncEnumerable<int> GetCurrentItemsSearch(CancellationToken ct);
}
