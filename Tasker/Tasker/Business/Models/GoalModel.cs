using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker.Business.Services;

namespace Tasker.Business.Models;
public partial record GoalModel(IGoalService goalService)
{
    
    public IListState<Goal> Goals => ListState.Async(this, goalService.GetGoals);
    public IState<Goal> NewGoal => State<Goal>.Value(this, EmptyGoal);

    private static Goal EmptyGoal() =>
        new Goal(Id: default, GoalName: string.Empty);

    public async ValueTask AddGoal(CancellationToken ct = default)
    {
        var newGoal = (await NewGoal)!;
        var newId = await goalService.AddGoal(newGoal, ct);

        await Goals.AddAsync(newGoal with { Id = newId }, ct);
        await NewGoal.UpdateAsync(current => EmptyGoal(), ct);
    }

    public async ValueTask RemoveGoal(Goal goal, CancellationToken ct = default)
    {
        var goalId = goal.Id;
        await goalService.RemoveGoal(goalId, ct);
        await Goals.RemoveAllAsync(g => g.Id == goalId, ct);
    }

    
}
