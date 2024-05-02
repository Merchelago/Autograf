using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Uno;
using Uno.Extensions.Reactive;
using Windows.UI;

namespace BlueT.Services;
public partial record Device(int Id, string DeviceName, string DeviceType);
public class BTService : IBTService
{
    private int nameId;
    private readonly int _allDevices = 50;
    private int _serchedDevices = 0;
    private readonly Signal _refreshList = new();
    private readonly Signal _refreshDeletedCreatedDevice = new();
    private readonly Signal _refreshColorDevice = new();
    private readonly Signal _refreshHistory = new();
    private List<Device> _devices = [];
    private List<string> _history = [];
    private Device _device = new(0,"","");
    private System.Drawing.Color _color;

    public BTService() {
        Task.Run(CreateDevicesAsync, CancellationToken.None);
        Task.Run(DeleteDevicesAsync, CancellationToken.None);
    }
    public Signal RefreshList  => _refreshList;

    public Signal RefreshDeletedCreatedDevice => _refreshDeletedCreatedDevice;

    public Signal RefreshColorDevice => _refreshColorDevice;

    public Signal RefreshHistory => _refreshHistory;

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
            _device = new(id, deviceName, deviceType);
            _history.Add($"Устройство создано: {id} {deviceName} {deviceType}");
            _refreshHistory.Raise();
            _color = System.Drawing.Color.Green;
            _refreshColorDevice.Raise();
            _refreshDeletedCreatedDevice.Raise();
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
            _device = _devices[indexToRemove];
            _history.Add($"Устройство Удалено: {_device.Id} {_device.DeviceName} {_device.DeviceType}");
            _refreshHistory.Raise();
            _color = System.Drawing.Color.Red;
            _refreshColorDevice.Raise();
            _refreshDeletedCreatedDevice.Raise();
            _devices.RemoveAt(indexToRemove);

            // Пересчитываем ID
            for (int i = indexToRemove; i < _devices.Count; i++)
            {
                _devices[i] = _devices[i] with { Id = i + 1 };
            }
            _refreshList.Raise();
        }
    }
    public async Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return _device;
    }
    public async Task<string> GetCreatedDeletedColorDeviceAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return _color.Name;
    }
    public async Task<ImmutableList<string>> GetHistoryAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);
        return [.. _history];
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
        string temp = "";
        if (searchTerm != temp) { 
            await Task.Delay(100, ct); // имитация поиска
            var result = _devices.Where(d =>
                d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
            _serchedDevices = result.ToList().Count;
            _history.Add($"Был использован поиск: {searchTerm}, найдено {_serchedDevices} устройств");
            return result.ToImmutableList();
        }
        else
        {
            _refreshHistory.Raise();
            temp = searchTerm;
            _serchedDevices = _devices.Count;
        }
        return [.. _devices];
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
