using Tasker.Business.Services;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Tasker.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        this.DataContext = new BindableGoalModel(new GoalService());
        //MainFrame.Navigate(typeof(GoalPage));
    }

    private void NavigateToCreateGoal(object sender, RoutedEventArgs e)
    {
        if (!(MainFrame.Content is CreateGoalPage))
        {
            var goalModel = this.DataContext;

            // Установка DataContext для передачи данных на следующую страницу
            this.DataContext = goalModel;
            MainFrame.Navigate(typeof(CreateGoalPage), this.DataContext);
        }
    }
    private void NavigateToMain(object sender, RoutedEventArgs e)
    {
        if (!(MainFrame.Content is GoalPage))
        {
            var goalModel = this.DataContext;

            this.DataContext = goalModel;
            MainFrame.Navigate(typeof(GoalPage), this.DataContext);
        }
    }
    private void NavigateToGoalPagination(object sender, RoutedEventArgs e)
    {
        if (!(MainFrame.Content is GoalPaginationPage))
        {
            var goalModel = this.DataContext;

            this.DataContext = goalModel;
            MainFrame.Navigate(typeof(GoalPaginationPage), this.DataContext);
        }
    }
}
