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
    private HttpClient _httpClient = new HttpClient();
    public async ValueTask<ImmutableList<Goal>> GetGoals(CancellationToken ct = default)
    {
        string apiUrl = "http://vhrweb.ru:8000/tasks";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var goals = JsonSerializer.Deserialize<Goal[]>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _goals.AddRange(goals);
                return [.. _goals];
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Произошла ошибка при выполнении запроса", ex);
            
        }
        return ImmutableList<Goal>.Empty;

        // return [.. _goals];
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

    public async ValueTask<IImmutableList<Goal>> GetGoalAsync(uint pageSize, uint firstItemIndex, CancellationToken ct)
    {
        var (size, count) = ((int)pageSize, (int)firstItemIndex);
        string apiUrl = "http://vhrweb.ru:8000/tasks";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var goals = JsonSerializer.Deserialize<Goal[]>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _goals.AddRange(goals);
                return _goals
                    .Skip(count)
                    .Take(size)
                    .ToImmutableList();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Произошла ошибка при выполнении запроса", ex);

        }
        return ImmutableList<Goal>.Empty;

    }

    public async ValueTask<uint> GetPageCount(uint pageSize, CancellationToken ct)=> 
        (uint)Math.Ceiling(_goals.Count / (double)pageSize);
    

    public async ValueTask<(IImmutableList<Goal> CurrentPage, int? NextGoalIdCursor)> GetGoalAsync(int? goalIdCursor, uint pageSize, CancellationToken ct)
    {
        string apiUrl = "http://vhrweb.ru:8000/tasks";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var goals = JsonSerializer.Deserialize<Goal[]>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _goals.AddRange(goals);
                var collection = _goals
                    .Where(goal => goal.Id >= goalIdCursor.GetValueOrDefault())
                    .Take((int)pageSize + 1)
                    .ToImmutableList();

                var noMoreItems = collection.Count <= pageSize;

                // use the last item as the cursor of next page, if it exceeds the page-size
                var lastIndex = noMoreItems ? ^0 : ^1;
                var nextGoalIdCursor = noMoreItems ? default(int?) : collection[^1].Id;

                return (CurrentPage: collection[..lastIndex].ToImmutableList(), NextPersonIdCursor: nextGoalIdCursor);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Произошла ошибка при выполнении запроса", ex);

        }
        ....
    }

    private List<Goal> _goals = new() { 
        new(1, "nake"),
        new(2, "have")
    };
}
