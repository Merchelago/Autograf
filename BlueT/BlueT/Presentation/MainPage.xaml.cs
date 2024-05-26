using BlueT.Services;
using Microsoft.Win32.SafeHandles;

namespace BlueT.Presentation;

public sealed partial class MainPage
{
    readonly BtService _btService;
    
    public MainPage()
    {
        InitializeComponent();
        _btService = new BtService();
        DataContext = new BindableBtModel(_btService); 
    }
}
