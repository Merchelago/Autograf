using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueT.Services;
public interface IBTService
{
    Task CreateDevicesAsync();
    Task DeleteDevicesAsync();
    IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct);
    Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct);

}
