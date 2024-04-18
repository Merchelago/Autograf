using Tasker.Business.Services;

namespace Tasker;

public class App : Application
{
    protected Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif

                .ConfigureServices((context, services) =>
                {
                    // TODO: Register your services
                    //services.AddSingleton<IMyService, MyService>();
                    services.AddSingleton<IGoalService, GoalService>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
                .UseSerialization()
            ) ;
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.EnableHotReload();
#endif

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, GoalModel>(),
            new ViewMap<GoalPage, GoalModel>(),
            new ViewMap<CreateGoalPage, GoalModel>(),
            new ViewMap<GoalPaginationPage, GoalModel>()
        ) ;

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested: new RouteMap[]
                {
                    new RouteMap("Main", View: views.FindByViewModel<GoalModel>()),
                    new RouteMap("Goal", View: views.FindByViewModel<GoalModel>()),
                    new RouteMap("CreateGoal", View: views.FindByViewModel<GoalModel>()),
                    new RouteMap("GoalPagination", View: views.FindByViewModel<GoalModel>())
                }
            )
        );
    }
}
