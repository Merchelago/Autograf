using System.Globalization;
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
    private readonly Signal _refreshHistory = new(); // сигнал на обновление списка найденных устройств
    private List<Device> _devices = []; // список устройств
    private List<string> _history = []; // список истории
    private ImmutableList<Device> _searchResult = []; // список найденных устройств
    private Device _device = new(0,"",""); // обьект устройсва для вывода созданного или удаленного устройства
    private string _tempTerm = "";
    private string _searchTerm = "";
    private int _devicesCount;

    public BtService() {
        token = source.Token;
        Task.Run(()=> CreateDevicesAsync(token), token);
        Task.Run(()=> DeleteDevicesAsync(token), token);
    }
    public CancellationTokenSource source = new CancellationTokenSource();
    public CancellationToken token;
    public Signal RefreshList  => _refreshList;
    public Signal RefreshDeletedCreatedDevice => _refreshDeletedCreatedDevice;
    public Signal RefreshHistory => _refreshHistory;
    public string SearchTerm { get => _searchTerm; set => _searchTerm = value; }
    public async Task Refresh(string searchTerm, CancellationToken ct)
    {
        await Task.Run(() => _refreshList.Raise(), ct);
        await Task.Run(() =>
        {
            var result = _devices.Where(d =>
                    d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                    d.DeviceType.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
            // Обновляем количество найденных устройств
            _serchedDevices = result.Count();
            // Добавляем историю поиска
            _history.Insert(0, $"{DateTime.Now:H:mm:ss} || Поиск: введено-> \"{searchTerm}\", найдено {_serchedDevices} устройств");
            _refreshHistory.Raise();
        }, ct);
    }
    
       
    public async Task CreateDevicesAsync(CancellationToken ct) // Метод для создания устройств 
    {
        while (!ct.IsCancellationRequested)
        {
            if (_devices.Count == _allDevices) continue;
            await Task.Delay(2000, ct);
            var random = new Random();
            string deviceType = RandomDeviceType(random);
            string deviceName = CreateName();
            int id = _devices.Count + 1;
            _devicesCount = _devices.Count;
            _device = new(id, deviceName, deviceType);
            _history.Insert(0, $"{DateTime.Now:H:mm:ss} || Создано: {id} {deviceName} {deviceType}");
            _devices.Add(new Device(id, deviceName, deviceType));
            _refreshList.Raise();
            _refreshHistory.Raise();
            _refreshDeletedCreatedDevice.Raise();
        }
        
    }

    public async Task DeleteDevicesAsync(CancellationToken ct) // Метод для удаления устройств 
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(13000, ct);
            if (_devices.Count == 0)
                return;
            var random = new Random();
            int indexToRemove = random.Next(_devices.Count);
            _device = _devices[indexToRemove];
            _history.Insert(0,$"{DateTime.Now:H:mm:ss} || Удалено: {_device.Id} {_device.DeviceName} {_device.DeviceType}");
            _devices.RemoveAt(indexToRemove);
            // Пересчитываем ID
            for (int i = indexToRemove; i < _devices.Count; i++)
            {
                _devices[i] = _devices[i] with { Id = i + 1 };
            }
            _refreshList.Raise();
            _refreshHistory.Raise();
            _refreshDeletedCreatedDevice.Raise();
        }
    }

    public async Task<Device> GetCreatedDeletedDeviceAsync(CancellationToken ct) // Метод для возращения только что удаленного или созданного устройства
    {
        
        return await Task.FromResult(_device);
    }

    public async Task<ImmutableList<string>> GetHistoryAsync(CancellationToken ct) 
    {
        
        return await Task.FromResult(_history.ToImmutableList());
    }

    public async Task<ImmutableList<Device>> ScanDevicesAsync(CancellationToken ct)
    {
        return await Task.FromResult( _devices.ToImmutableList());
    }

    public async Task<ImmutableList<Device>> GetDevicesAsync(CancellationToken ct)
    {
        return await Task.FromResult(_devices.ToImmutableList());
    }
    public async Task<ImmutableList<Device>> GetDevicesSearchAsync( CancellationToken ct)
    {
        return await Task.FromResult(_devices.ToImmutableList());
    }
    /*public async Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Если поисковый запрос пустой, возвращаем основной список устройств
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _serchedDevices = _devices.Count;
                return await Task.FromResult(_devices.ToImmutableList());
            }

            if (searchTerm != _tempTerm)
            {
                // Выполняем поиск устройств
                var result = _devices.Where(d =>
                    d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                    d.DeviceType.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
                // Обновляем количество найденных устройств
                _serchedDevices = result.Count();
                // Добавляем историю поиска
                _history.Insert(0, $"{DateTime.Now:H:mm:ss} || Поиск: введено-> \"{searchTerm}\", найдено {_serchedDevices} устройств");
                _tempTerm = searchTerm;
                _searchResult = [.. result];
                _refreshHistory.Raise();
                return await Task.FromResult(result.ToImmutableList());
            }
            return await Task.FromResult(_searchResult.ToImmutableList());
        }
        return await Task.FromResult(_searchResult.ToImmutableList());
    }*/
    /*
    public async Task<ImmutableList<Device>> GetDevicesSearchAsync(string searchTerm, CancellationToken ct)
    {
        while(!ct.IsCancellationRequested)
        // Если поисковый запрос пустой, возвращаем основной список устройств
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _serchedDevices = _devices.Count;
           return await Task.FromResult(_devices.ToImmutableList());
        }

        if (searchTerm != _tempTerm)
        {
            // Выполняем поиск устройств
            var result = _devices.Where(d =>
                d.DeviceName.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase) ||
                d.DeviceType.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase));
            // Обновляем количество найденных устройств
            _serchedDevices = result.Count();
            // Добавляем историю поиска
            _history.Insert(0, $"{DateTime.Now:H:mm:ss} || Поиск: введено-> \"{searchTerm}\", найдено {_serchedDevices} устройств");
            _tempTerm = searchTerm;
            _searchResult = [.. result];
            _refreshHistory.Raise();
            return await Task.FromResult(result.ToImmutableList());
        }
        return await Task.FromResult(_searchResult.ToImmutableList());
    }*/
    public async ValueTask<int> GetAllItems(CancellationToken ct)
    {
        
        return await Task.FromResult(_allDevices);
        
    }

    public async ValueTask<int> GetCurrentItems( CancellationToken ct)
    {
         return await Task.FromResult(_devicesCount);
        
    }
    public async ValueTask<int> GetCurrentItemsSearch(CancellationToken ct)
    {
        return await Task.FromResult( _serchedDevices);
        
    }

    private string CreateName()
    {
        int id = nameId + 1;
        nameId++;
        string formattedId = id.ToString("0000");
        return formattedId;
    }

    private static string RandomDeviceType(Random random)
    {
        string[] deviceTypes = ["Phone", "Tablet", "Laptop", "Desktop", "Smartwatch"];
        return deviceTypes[random.Next(deviceTypes.Length)];
    }
}
