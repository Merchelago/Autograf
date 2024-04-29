using BlueT.Services;

namespace BlueT.Presentation;

public sealed partial class MainPage
{
    public MainPage()
    {
        this.InitializeComponent();
        this.DataContext = new BindableBTModel(new BTService());
    }
}
