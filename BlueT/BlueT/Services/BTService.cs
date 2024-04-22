using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Uno;

namespace BlueT.Services;
public partial record Device(int Id, String DeviceName, string DeviceType);
public class BTService : IBTService
{
    private List<Device> _devices = new() { 
        new(1, "DeviceName", "DeviceType"),
    };
    CancellationTokenSource source = new CancellationTokenSource();
    
    public BTService() {
        Task.Run(DeleteDevicesAsync, CancellationToken.None);
    }
    public async Task CreateDevicesAsync()
    {
        if (_devices.Count == 10) return;
        await Task.Delay(2000);
        var random = new Random();
        string deviceType = RandomDeviceType(random);
        string deviceName = RandomString(random, 6);
        int id = _devices.Count + 1;
            
        _devices.Add(new Device(id, deviceName, deviceType));

    }
    public async Task DeleteDevicesAsync()
    {
        while (true)
        {
            await Task.Delay(3000);
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
            await Task.Run(CreateDevicesAsync);      
        }
    }

    public async Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm)
    {
        await Task.Delay(1000); // имитация поиска
        if (searchTerm == "") return [];
        searchTerm = searchTerm.ToLower();
        var result = _devices.Where(d =>
            d.DeviceName.Contains(searchTerm));
        return result.ToImmutableList();
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
