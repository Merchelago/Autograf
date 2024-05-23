using BlueT.Services;
using Microsoft.Win32.SafeHandles;

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

   /* private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _btService.RefreshList.Raise();
    }

    private  void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        _btService.source.Cancel();
        _btService.RefreshList.Raise();

        
        Console.WriteLine($"{sender.Text}");
    }

    private void FeedView_SizeChanged(object sender, SizeChangedEventArgs args)
    {

    }*/
}
