using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasker.Business.Services;
public partial record Goal(int Id, string GoalName);
public class GoalService : IGoalService
{
    public async ValueTask<ImmutableList<Goal>> GetGoals(CancellationToken ct = default)
    {
        await Task.Delay(500, ct);

        return [.. _goals];
    }

    public async ValueTask<int> AddGoal(Goal goal, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var newId = GenerateNewId();

        _goals.Add(goal with {Id = newId});
        return newId;
    }

    public ValueTask UpdateGoal(int goalId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask RemoveGoal(int goalId, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        _goals.RemoveAll(goal => goal.Id == goalId);
    }

    private int GenerateNewId() => _goals.DefaultIfEmpty().Max(goal => goal?.Id ?? default) + 1;

    private List<Goal> _goals = new() { 
        new(1, "nake"),
        new(2, "have")
    };
}
