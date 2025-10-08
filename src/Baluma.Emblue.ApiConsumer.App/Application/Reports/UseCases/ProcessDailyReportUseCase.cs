using System.IO;
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
    private readonly IFileStorage _fileStorage;
    private readonly IDailyReportRepository _dailyReportRepository;
    private readonly IDuplicateDataHandler _duplicateDataHandler;
    private readonly ITaskExecutionLogRepository _taskExecutionLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ProcessDailyReportUseCase> _logger;

    public ProcessDailyReportUseCase(
        IAutomaticReportClient automaticReportClient,
        IEnumerable<IReportContentParser> parsers,
        IFileStorage fileStorage,
        IDailyReportRepository dailyReportRepository,
        IDuplicateDataHandler duplicateDataHandler,
        ITaskExecutionLogRepository taskExecutionLogRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<ProcessDailyReportUseCase> logger)
    {
        _automaticReportClient = automaticReportClient;
        _parsers = parsers.ToDictionary(parser => parser.ReportType);
        _fileStorage = fileStorage;
        _dailyReportRepository = dailyReportRepository;
        _duplicateDataHandler = duplicateDataHandler;
        _taskExecutionLogRepository = taskExecutionLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(DateOnly? date, bool isAutomaticExecution, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var parameters = date.HasValue ? targetDate.ToString("yyyy-MM-dd") : "automatic";
        var executionLog = TaskExecutionLog.Start("ProcesarReporteDiario", parameters, _dateTimeProvider.UtcNow);
        await _taskExecutionLogRepository.AddAsync(executionLog, cancellationToken);

        try
        {
            if (await _dailyReportRepository.ExistsDataForDateAsync(targetDate, cancellationToken))
            {
                if (isAutomaticExecution)
                {
                    _logger.LogInformation("Existing data found for {Date}. Removing previous data (automatic mode).", targetDate);
                    await _dailyReportRepository.DeleteDataForDateAsync(targetDate, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Existing data found for {Date}. Requesting operator confirmation to overwrite.", targetDate);
                    var shouldReplace = await _duplicateDataHandler.ConfirmReplacementAsync(targetDate, cancellationToken);
                    if (!shouldReplace)
                    {
                        executionLog.MarkCancelled($"El operador canceló la ejecución porque ya existen datos para {targetDate:yyyy-MM-dd}.", _dateTimeProvider.UtcNow);
                        await _taskExecutionLogRepository.UpdateAsync(executionLog, cancellationToken);
                        _logger.LogWarning("Processing cancelled by operator because data already existed for {Date}.", targetDate);
                        return;
                    }

                    _logger.LogInformation("Operator approved overwriting data for {Date}. Removing previous records.", targetDate);
                    await _dailyReportRepository.DeleteDataForDateAsync(targetDate, cancellationToken);
                }
            }

            var descriptors = await _automaticReportClient.GetDailyReportsAsync(targetDate, cancellationToken);
            var processedCount = 0;

            foreach (var descriptor in descriptors)
            {
                var (stream, downloadedFromApi) = await GetReportStreamAsync(descriptor, cancellationToken);
                await using var reportStream = stream;

                if (downloadedFromApi)
                {
                    await _fileStorage.SaveAsync(reportStream, descriptor.FileName, cancellationToken);
                }

                if (!_parsers.TryGetValue(descriptor.ReportType, out var parser) || parser is null)
                {
                    _logger.LogWarning("No parser registered for report type {ReportType} ({FileName}). Report stored but not processed.", descriptor.ReportType, descriptor.FileName);
                    continue;
                }

                var taskExecutionFileId = await _dailyReportRepository.EnsureReportFileAsync(
                    descriptor,
                    targetDate,
                    executionLog.Id,
                    cancellationToken);

                if (reportStream.CanSeek)
                {
                    reportStream.Seek(0, SeekOrigin.Begin);
                }

                await parser.ParseAndPersistAsync(reportStream, taskExecutionFileId, cancellationToken);
                await _dailyReportRepository.MarkReportFileAsProcessedAsync(taskExecutionFileId, _dateTimeProvider.UtcNow, cancellationToken);
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

    private async Task<(Stream Stream, bool DownloadedFromApi)> GetReportStreamAsync(AutomaticReportFileDescriptor descriptor, CancellationToken cancellationToken)
    {
        try
        {
            var stream = await _automaticReportClient.DownloadReportAsync(descriptor, cancellationToken);
            return (stream, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download report {FileName} (ID: {FileId}). Attempting to use a previously stored copy.", descriptor.FileName, descriptor.FileId);

            if (!await _fileStorage.ExistsAsync(descriptor.FileName, cancellationToken))
            {
                throw new InvalidOperationException($"No se pudo descargar el archivo {descriptor.FileName} y no existe una copia local previa.", ex);
            }

            _logger.LogInformation("Using previously stored report {FileName} (ID: {FileId}).", descriptor.FileName, descriptor.FileId);
            var localStream = await _fileStorage.OpenReadAsync(descriptor.FileName, cancellationToken);
            return (localStream, false);
        }
    }
}
