using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Application.Reports.Parsers;
using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Domain.TaskExecution;
using Microsoft.Extensions.Logging;

namespace Baluma.Emblue.ApiConsumer.Application.Reports.UseCases;

public sealed class ProcessDailyReportUseCase : IProcessDailyReportUseCase
{
    private readonly IAutomaticReportClient _automaticReportClient;
    private readonly IReadOnlyDictionary<AutomaticReportType, IReportContentParser> _parsers;
    private readonly ITaskExecutionLogRepository _taskExecutionLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ProcessDailyReportUseCase> _logger;

    public ProcessDailyReportUseCase(
        IAutomaticReportClient automaticReportClient,
        IEnumerable<IReportContentParser> parsers,
        ITaskExecutionLogRepository taskExecutionLogRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessDailyReportUseCase> logger)
    {
        _automaticReportClient = automaticReportClient;
        _parsers = parsers.ToDictionary(parser => parser.ReportType);
        _taskExecutionLogRepository = taskExecutionLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var parameters = date.HasValue ? targetDate.ToString("yyyy-MM-dd") : "automatic";
        var executionLog = TaskExecutionLog.Start("ProcesarReporteDiario", parameters, _dateTimeProvider.UtcNow);
        await _taskExecutionLogRepository.AddAsync(executionLog, cancellationToken);

        try
        {
            var descriptors = await _automaticReportClient.GetDailyReportsAsync(targetDate, cancellationToken);
            var processedCount = 0;

            foreach (var descriptor in descriptors)
            {
                if (!_parsers.TryGetValue(descriptor.ReportType, out var parser) || parser is null)
                {
                    _logger.LogWarning("No parser registered for report type {ReportType} ({FileName})", descriptor.ReportType, descriptor.FileName);
                    continue;
                }

                await using var stream = await _automaticReportClient.DownloadReportAsync(descriptor, cancellationToken);
                await parser.ParseAndPersistAsync(stream, cancellationToken);
                processedCount++;
            }

            executionLog.MarkSucceeded($"Processed {processedCount} report(s).", _dateTimeProvider.UtcNow);
            await _taskExecutionLogRepository.UpdateAsync(executionLog, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing daily reports for {Date}", targetDate);
            executionLog.MarkFailed(ex.Message, _dateTimeProvider.UtcNow);
            await _taskExecutionLogRepository.UpdateAsync(executionLog, cancellationToken);
            throw;
        }
    }
}
