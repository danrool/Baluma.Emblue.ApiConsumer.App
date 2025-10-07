using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;

namespace Baluma.Emblue.ApiConsumer.Domain.TaskExecution;

public sealed class TaskExecutionFile
{
    private TaskExecutionFile()
    {
    }

    public int Id { get; private set; }
    public int TaskExecutionLogId { get; private set; }
    public TaskExecutionLog TaskExecutionLog { get; private set; } = null!;
    public AutomaticReportType ReportType { get; private set; }
    public int FileId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public DateOnly ReportDate { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    public static TaskExecutionFile Create(
        int taskExecutionLogId,
        AutomaticReportType reportType,
        int fileId,
        string fileName,
        DateOnly reportDate,
        DateTime createdAtUtc)
    {
        return new TaskExecutionFile
        {
            TaskExecutionLogId = taskExecutionLogId,
            ReportType = reportType,
            FileId = fileId,
            FileName = fileName,
            ReportDate = reportDate,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void UpdateMetadata(
        int taskExecutionLogId,
        string fileName,
        DateOnly reportDate,
        DateTime createdAtUtc)
    {
        TaskExecutionLogId = taskExecutionLogId;
        FileName = fileName;
        ReportDate = reportDate;
        CreatedAtUtc = createdAtUtc;
        ProcessedAtUtc = null;
    }

    public void MarkProcessed(DateTime processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
    }
}
