namespace Baluma.Emblue.ApiConsumer.Domain.TaskExecution;

public sealed class TaskExecutionLog
{
    private TaskExecutionLog()
    {
    }

    public int Id { get; private set; }
    public string TaskName { get; private set; } = string.Empty;
    public string? Parameters { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public ICollection<TaskExecutionFile> Files { get; private set; } = new List<TaskExecutionFile>();

    public static TaskExecutionLog Start(string taskName, string? parameters, DateTime startedAtUtc)
    {
        return new TaskExecutionLog
        {
            TaskName = taskName,
            Parameters = parameters,
            StartedAtUtc = startedAtUtc,
            Status = "Started"
        };
    }

    public void MarkSucceeded(string message, DateTime completedAtUtc)
    {
        Status = "Succeeded";
        Message = message;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkFailed(string message, DateTime completedAtUtc)
    {
        Status = "Failed";
        Message = message;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkCancelled(string message, DateTime completedAtUtc)
    {
        Status = "Cancelled";
        Message = message;
        CompletedAtUtc = completedAtUtc;
    }
}
