using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasker.Business.Services;
public interface IGoalService
{
    ValueTask<ImmutableList<Goal>> GetGoals(CancellationToken ct = default);
    ValueTask<int> AddGoal(Goal goal, CancellationToken ct = default);
    ValueTask UpdateGoal(int goalId, CancellationToken ct = default);
    ValueTask RemoveGoal(int goalId, CancellationToken ct = default);
}
