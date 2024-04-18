using Tasker.Business.Services;
namespace Tasker.Presentation;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GoalPaginationPage : Page
{
    public GoalPaginationPage()
    {
        this.InitializeComponent();
        this.DataContext = new BindableGoalModel(new GoalService());
    }
}
