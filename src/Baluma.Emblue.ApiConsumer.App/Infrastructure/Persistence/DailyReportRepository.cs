using System.Linq;
using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Domain.Reports;
using Baluma.Emblue.ApiConsumer.Domain.TaskExecution;
using Microsoft.EntityFrameworkCore;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;

public sealed class DailyReportRepository : IDailyReportRepository
{
    private readonly ApiConsumerDbContext _dbContext;

    public DailyReportRepository(ApiConsumerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> EnsureReportFileAsync(
        AutomaticReportFileDescriptor descriptor,
        DateOnly reportDate,
        int taskExecutionLogId,
        CancellationToken cancellationToken)
    {
        var taskExecutionFile = await _dbContext.TaskExecutionFiles
            .FirstOrDefaultAsync(
                file => file.FileId == descriptor.FileId && file.ReportType == descriptor.ReportType,
                cancellationToken);

        if (taskExecutionFile is null)
        {
            taskExecutionFile = TaskExecutionFile.Create(
                taskExecutionLogId,
                descriptor.ReportType,
                descriptor.FileId,
                descriptor.FileName,
                reportDate,
                descriptor.CreatedAtUtc);

            await _dbContext.TaskExecutionFiles.AddAsync(taskExecutionFile, cancellationToken);
        }
        else
        {
            taskExecutionFile.UpdateMetadata(taskExecutionLogId, descriptor.FileName, reportDate, descriptor.CreatedAtUtc);
            _dbContext.TaskExecutionFiles.Update(taskExecutionFile);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return taskExecutionFile.Id;
    }

    public async Task MarkReportFileAsProcessedAsync(int taskExecutionFileId, DateTime processedAtUtc, CancellationToken cancellationToken)
    {
        await _dbContext.TaskExecutionFiles
            .Where(file => file.Id == taskExecutionFileId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(file => file.ProcessedAtUtc, processedAtUtc),
                cancellationToken);
    }

    public async Task SaveDailyActivityDetailsAsync(
        int taskExecutionFileId,
        IEnumerable<DailyActivityDetail> details,
        CancellationToken cancellationToken)
    {
        await _dbContext.DailyActivityDetails
            .Where(detail => detail.TaskExecutionFileId == taskExecutionFileId)
            .ExecuteDeleteAsync(cancellationToken);

        var detailList = details.ToList();
        if (detailList.Count == 0)
        {
            return;
        }

        foreach (var detail in detailList)
        {
            detail.TaskExecutionFileId = taskExecutionFileId;
        }

        await _dbContext.DailyActivityDetails.AddRangeAsync(detailList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveDailyActionSummariesAsync(
        int taskExecutionFileId,
        IEnumerable<DailyActionSummary> summaries,
        CancellationToken cancellationToken)
    {
        await _dbContext.DailyActionSummaries
            .Where(summary => summary.TaskExecutionFileId == taskExecutionFileId)
            .ExecuteDeleteAsync(cancellationToken);

        var summaryList = summaries.ToList();
        if (summaryList.Count == 0)
        {
            return;
        }

        foreach (var summary in summaryList)
        {
            summary.TaskExecutionFileId = taskExecutionFileId;
        }

        await _dbContext.DailyActionSummaries.AddRangeAsync(summaryList, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsDataForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var hasDetails = await (from detail in _dbContext.DailyActivityDetails
                join taskExecutionFile in _dbContext.TaskExecutionFiles
                    on detail.TaskExecutionFileId equals taskExecutionFile.Id
                where taskExecutionFile.ReportDate == date
                select detail.Id)
            .AnyAsync(cancellationToken);

        if (hasDetails)
        {
            return true;
        }

        var hasSummaries = await (from summary in _dbContext.DailyActionSummaries
                join taskExecutionFile in _dbContext.TaskExecutionFiles
                    on summary.TaskExecutionFileId equals taskExecutionFile.Id
                where taskExecutionFile.ReportDate == date
                select summary.Id)
            .AnyAsync(cancellationToken);

        return hasSummaries;
    }

    public async Task DeleteDataForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        await _dbContext.DailyActivityDetails
            .Where(detail =>
                (detail.ActivityDate.HasValue && detail.ActivityDate >= start && detail.ActivityDate < end) ||
                (detail.SendDate.HasValue && detail.SendDate >= start && detail.SendDate < end))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.DailyActionSummaries
            .Where(summary => summary.Date.HasValue && summary.Date >= start && summary.Date < end)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
