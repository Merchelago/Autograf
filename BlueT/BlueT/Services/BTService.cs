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
    private int nameId = 0;
    private string? _searchName = string.Empty;
    public BTService() {
        Task.Run(DeleteDevicesAsync, CancellationToken.None);
    }

    private List<Device> _devices = new() {
        new(1, "DeviceName", "DeviceType"),
    };

    public string SearchName { get => _searchName; set => _searchName = value; }

    public async Task CreateDevicesAsync()
    {
        if (_devices.Count == 50) return;
        await Task.Delay(2000);
        var random = new Random();
        string deviceType = RandomDeviceType(random);
        string deviceName = CreateName();
        int id = _devices.Count + 1;
            
        _devices.Add(new Device(id, deviceName, deviceType));

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
        }
    }
    public async IAsyncEnumerable<ImmutableList<Device>> ScanDevicesAsync([EnumeratorCancellation]CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            yield return [.. _devices];
            await Task.Run(CreateDevicesAsync, ct);      
        }
    }

    public async IAsyncEnumerable<ImmutableList<Device>> SerchDevicesAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_searchName == "") yield return _devices.ToImmutableList();
            await Task.Delay(10, ct); // имитация поиска
            var result = _devices.Where(d =>
                d.DeviceName.Contains(_searchName, StringComparison.CurrentCultureIgnoreCase));
            yield return result.ToImmutableList();
        }
    }

    public async Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct)
    {

        if (searchTerm == "")  return _devices.ToImmutableList();
        await Task.Delay(10, ct); // имитация поиска
        var result = _devices.Where(d =>
            d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
        return result.ToImmutableList();

    }
    private string CreateName()
    {
        int id = nameId + 1;
        nameId++;
        string formattedId = id.ToString("0000");
        return formattedId;
    }
    private string RandomString(Random random, int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string RandomDeviceType(Random random)
    {
        string[] deviceTypes = { "Phone", "Tablet", "Laptop", "Desktop", "Smartwatch" };
        return deviceTypes[random.Next(deviceTypes.Length)];
    }

}
