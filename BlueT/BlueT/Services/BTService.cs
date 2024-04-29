using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Uno;
using Uno.Extensions.Reactive;

namespace BlueT.Services;
public partial record Device(int Id, string DeviceName, string DeviceType);
public class BTService : IBTService
{
    private int nameId;
    private readonly int _allDevices = 50;
    private int _serchedDevices = 0;
    private readonly Signal _refreshList = new();
    private List<Device> _devices = [];
    public BTService() {
        Task.Run(CreateDevicesAsync, CancellationToken.None);
        Task.Run(DeleteDevicesAsync, CancellationToken.None);
    }
    public Signal RefreshList  => _refreshList;

    public async Task CreateDevicesAsync()
    {
        while (true)
        {
            if (_devices.Count == _allDevices) continue;
            await Task.Delay(2000);
            var random = new Random();
            string deviceType = RandomDeviceType(random);
            string deviceName = CreateName();
            int id = _devices.Count + 1;

            _devices.Add(new Device(id, deviceName, deviceType));
            _refreshList.Raise();
        }
        
    }
    public async Task DeleteDevicesAsync()
    {
        while (true)
        {
            await Task.Delay(13000);
            if (_devices.Count == 0)
                return;
            var random = new Random();
            int indexToRemove = random.Next(_devices.Count);
            _devices.RemoveAt(indexToRemove);

            // Пересчитываем ID
            for (int i = indexToRemove; i < _devices.Count; i++)
            {
                _devices[i] = _devices[i] with { Id = i + 1 };
            }
            _refreshList.Raise();
        }
    }

    public async IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync([EnumeratorCancellation]CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(100, ct);
            yield return [.. _devices];
            _refreshList.Raise();
        }
    }
    public async Task<ImmutableList<Device>> GetDevicesAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return [.._devices] ;
    }
    public async Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct)
    {
        _serchedDevices = 0;
        if (searchTerm == "") 
        { 
            _serchedDevices = _devices.Count;
            return [.. _devices]; 
        }

        await Task.Delay(100, ct); // имитация поиска
        var result = _devices.Where(d =>
            d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        _serchedDevices = result.ToList().Count;
        return result.ToImmutableList();

    }

    public async ValueTask<int> GetAllItems(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return _allDevices;
    }

    public async IAsyncEnumerable<int> GetCurrentItems([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(2000, ct);
            yield return _devices.Count;
        }
    }
    public async IAsyncEnumerable<int> GetCurrentItemsSearch([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(10, ct);
            yield return _serchedDevices;
        }
    }

    private string CreateName()
    {
        int id = nameId + 1;
        nameId++;
        string formattedId = id.ToString("0000");
        return formattedId;
    }

    private string RandomDeviceType(Random random)
    {
        string[] deviceTypes = { "Phone", "Tablet", "Laptop", "Desktop", "Smartwatch" };
        return deviceTypes[random.Next(deviceTypes.Length)];
    }

  
}
