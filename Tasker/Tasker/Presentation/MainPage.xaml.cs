using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Tasker.Business.Services;
using Windows.Foundation.Collections;

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
        if (!(MainFrame.Content is GoalPaginationPage))
        {
            var goalModel = this.DataContext;

            this.DataContext = goalModel;
            MainFrame.Navigate(typeof(GoalPaginationPage), this.DataContext);
        }
    }
}
