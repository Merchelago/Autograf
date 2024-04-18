using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker.Business.Services;
using Uno.Extensions.Reactive.Sources;

namespace Tasker.Business.Models;
public partial record GoalModel(IGoalService GoalService)
{
    const uint DefaultPageSize = 10;

    public IListFeed<Goal> GoalAuto =>
        ListFeed.AsyncPaginated((AsyncFunc<PageRequest, IImmutableList<Goal>>)(async (PageRequest pageRequest, CancellationToken ct) =>
            await GoalService.GetGoalAsync(pageSize: pageRequest.DesiredSize ?? DefaultPageSize, firstItemIndex: pageRequest.CurrentCount, ct)));

    public IFeed<uint> PageCount =>
        Feed.Async((AsyncFunc<uint>)(async (ct) => await GoalService.GetPageCount(DefaultPageSize, ct)));

    public IState<uint> CurrentPage => State.Value(this, () => 1u);

    public IListFeed<Goal> GoalManual =>
       CurrentPage.SelectAsync(async (currentPage, ct) =>
           await GoalService.GetGoalAsync(
               pageSize: DefaultPageSize,
               // currentPage argument as index based - subtracting 1
               firstItemIndex: (currentPage - 1) * DefaultPageSize, ct))
       .AsListFeed();

    public IListFeed<Goal> GoalCursor =>
    ListFeed<Goal>.AsyncPaginatedByCursor(
        // starting off with a blank Person, since the person list is to be ordered by name, any valid name will follow.
        firstPage: default(int?),
        // this will be automatically invoked by the ISupportIncrementalLoading the ListView supports
        getPage: (GetPage<int?, Goal>)(async (cursor, desiredPageSize, ct) =>
        {
            var result = await GoalService.GetGoalAsync(cursor, desiredPageSize ?? DefaultPageSize, ct);
            return new PageResult<int?, Goal>((IImmutableList<Goal>)result.CurrentPage, (int?)result.NextGoalIdCursor);
        }));

    public IListState<Goal> Goals => ListState.Async(this, GoalService.GetGoals);
    public IState<Goal> NewGoal => State<Goal>.Value(this, EmptyGoal);

    private static Goal EmptyGoal() =>
        new Goal(Id: default, GoalName: string.Empty);

    public async ValueTask AddGoal(CancellationToken ct = default)
    {
        var newGoal = (await NewGoal)!;
        var newId = await GoalService.AddGoal(newGoal, ct);

        await Goals.AddAsync(newGoal with { Id = newId }, ct);
        await NewGoal.UpdateAsync(current => EmptyGoal(), ct);
    }

    public async ValueTask RemoveGoal(Goal goal, CancellationToken ct = default)
    {
        var goalId = goal.Id;
        await GoalService.RemoveGoal(goalId, ct);
        await Goals.RemoveAllAsync(g => g.Id == goalId, ct);
    }

    
}
