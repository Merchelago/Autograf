using BlueT.Services;

namespace BlueT.Presentation;

public sealed partial class MainPage
{
    BtService _btService;
    public MainPage()
    {
        InitializeComponent();
        _btService = new BtService();
        DataContext = new BindableBtModel(_btService);
    }

    private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        _btService.RefreshList.Raise();
    }
}
