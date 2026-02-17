using System.Text.Json;

namespace TaskManagerBot;

public class TaskService
{
    private const string FilePath = "tasks.json";
    public TaskService()
{
    LoadFromFile();
}

    private Dictionary<long, List<TaskItem>> _tasks = new();

    public void AddTask(long chatId, string title)
    {
        if (!_tasks.ContainsKey(chatId))
            _tasks[chatId] = new List<TaskItem>();

        _tasks[chatId].Add(new TaskItem { Title = title });
        SaveToFile();
    }

    public List<TaskItem> GetTasks(long chatId)
    {
        if (!_tasks.ContainsKey(chatId))
            _tasks[chatId] = new List<TaskItem>();

        return _tasks[chatId];
    }

    public bool DeleteTask(long chatId, int index)
    {
        var tasks = GetTasks(chatId);

        if (index < 0 || index >= tasks.Count)
            return false;

        tasks.RemoveAt(index);
        SaveToFile();

        return true;
        
    }

    public bool CompleteTask(long chatId, int index)
    {
        var tasks = GetTasks(chatId);

        if (index < 0 || index >= tasks.Count)
            return false;

        tasks[index].IsCompleted = true;
        SaveToFile();

        return true;
    }
    private void LoadFromFile()
    {
        if (!File.Exists(FilePath))
            return;

        var json = File.ReadAllText(FilePath);

        var data = JsonSerializer.Deserialize<Dictionary<long, List<TaskItem>>>(json);

        if (data != null)
            _tasks = data;
    }
    private void SaveToFile()
    {
    var json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    File.WriteAllText(FilePath, json);
    }


}
