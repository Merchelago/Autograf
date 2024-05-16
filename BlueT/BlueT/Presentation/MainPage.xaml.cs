using BlueT.Services;

namespace BlueT.Presentation;

public sealed partial class MainPage
{
    public MainPage()
    {
        InitializeComponent();
        DataContext = new BindableBtModel(new BtService());
    }
}
