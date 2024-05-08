using System.Runtime.CompilerServices;

namespace BlueT.Services;
public partial record Device(int Id, string DeviceName, string DeviceType);
public class BtService : IBtService
{
    private int nameId; // Имя устройства
    private readonly int _allDevices = 50; // кол-во устройств
    private int _serchedDevices = 0; // кол-во найденных устройств
    private readonly Signal _refreshList = new(); // сигнал на обновление списка устройств
    private readonly Signal _refreshDeletedCreatedDevice = new();
    private readonly Signal _refreshColorDevice = new();
    private readonly Signal _refreshHistory = new(); // сигнал на обновление списка найденных устройств
    private List<Device> _devices = []; // список устройств
    private List<string> _history = []; // список найденных устройств
    private Device _device = new(0,"",""); // обьект устройсва для вывода созданного или удаленного устройства
    private System.Drawing.Color _color;

    public BtService() {
        Task.Run(CreateDevicesAsync, CancellationToken.None); // задача на добавление устройств в список
        Task.Run(DeleteDevicesAsync, CancellationToken.None); // задача на удаление устройств из списка
    }
    public Signal RefreshList  => _refreshList;

    public Signal RefreshDeletedCreatedDevice => _refreshDeletedCreatedDevice;

    public Signal RefreshColorDevice => _refreshColorDevice;

    public Signal RefreshHistory => _refreshHistory;

    public async Task CreateDevicesAsync() // Метод для создания устройств 
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
            _history.Insert(0, $"{DateTime.Now.ToLongTimeString()} Создано: {id} {deviceName} {deviceType}");
            _refreshHistory.Raise();
            _color = System.Drawing.Color.Green;
            _refreshColorDevice.Raise();
            _refreshDeletedCreatedDevice.Raise();
            _devices.Add(new Device(id, deviceName, deviceType));
            _refreshList.Raise();
        }
        
    }
    public async Task DeleteDevicesAsync() // Метод для удаления устройств 
    {
        while (true)
        {
            await Task.Delay(13000);
            if (_devices.Count == 0)
                return;
            var random = new Random();
            int indexToRemove = random.Next(_devices.Count);
            _device = _devices[indexToRemove];
            _history.Insert(0,$"{DateTime.Now.ToLongTimeString()} Удалено: {_device.Id} {_device.DeviceName} {_device.DeviceType}");
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
    public async Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct) // Метод для возращения только что удаленного или созданного устройства
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
