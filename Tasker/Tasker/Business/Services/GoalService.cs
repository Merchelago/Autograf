using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tasker.Business.Services;
public partial record Goal(int Id, string GoalName);
public class GoalService : IGoalService
{

    private List<Goal> _goals = new();

    public async ValueTask<List<Goal>> GoalsJsonGet()
    {

        // string apiUrl = "data.json";
        string url = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        using FileStream fileStream = File.OpenRead($"{url}\\data.json");
        var goals = await Task.Run(()=>JsonSerializer.Deserialize<Goal[]>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
        return [.. goals];
    }
    public async ValueTask<ImmutableList<Goal>> GoalsJsonGet1()
    {

        // string apiUrl = "data.json";
        string url = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        using FileStream fileStream = File.OpenRead($"{url}\\data.json");
        var goals = await Task.Run(() => JsonSerializer.Deserialize<Goal[]>(fileStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }));
        return [.. goals];
    }
    public void GoalsJsonSet()
    {
        
       // string apiUrl = "data.json";
        string url = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string jsonString = JsonSerializer.Serialize(_goals);
        File.WriteAllText($"{url}\\data.json", jsonString);
        
    }
    public async ValueTask<ImmutableList<Goal>> GetGoals(CancellationToken ct = default)
    {
        var goals = await Task.Run(() => GoalsJsonGet1());

        return [.. (await goals)];
        
    }

    public async ValueTask<int> AddGoal(Goal goal, CancellationToken ct = default)
    {
        await Task.Delay(500, ct);
        var newId = GenerateNewId();

        _goals.Add(goal with {Id = newId});
        GoalsJsonSet();
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
        GoalsJsonSet();
    }

    private int GenerateNewId() => _goals.DefaultIfEmpty().Max(goal => goal?.Id ?? default) + 1;

    public async ValueTask<IImmutableList<Goal>> GetGoalAsync(uint pageSize, uint firstItemIndex, CancellationToken ct)
    {
        var (size, count) = ((int)pageSize, (int)firstItemIndex);

        var coll = await Task.Run(()=> GoalsJsonGet());
        return coll.Result
            .Skip(count)
            .Take(size)
            .ToImmutableList();
    }

    public async ValueTask<uint> GetPageCount(uint pageSize, CancellationToken ct)=> 
        (uint)Math.Ceiling(_goals.Count / (double)pageSize);
    

    public async ValueTask<(IImmutableList<Goal> CurrentPage, int? NextGoalIdCursor)> GetGoalAsync(int? goalIdCursor, uint pageSize, CancellationToken ct)
    {
        await Task.Run(()=> GoalsJsonGet());
        var collection = _goals
            .Where(goal => goal.Id >= goalIdCursor.GetValueOrDefault())
            .Take((int)pageSize + 1)
            .ToArray();

        var noMoreItems = collection.Length <= pageSize;

        // use the last item as the cursor of next page, if it exceeds the page-size
        var lastIndex = noMoreItems ? ^0 : ^1;
        var nextGoalIdCursor = noMoreItems ? default(int?) : collection[^1].Id;

        return (CurrentPage: collection[..lastIndex].ToImmutableList(), NextGoalIdCursor: nextGoalIdCursor);
    }

    
}
