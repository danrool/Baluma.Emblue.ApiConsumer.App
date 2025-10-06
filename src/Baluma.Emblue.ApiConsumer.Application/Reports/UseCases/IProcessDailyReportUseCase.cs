namespace Baluma.Emblue.ApiConsumer.Application.Reports.UseCases;

public interface IProcessDailyReportUseCase
{
    Task ExecuteAsync(DateOnly? date, CancellationToken cancellationToken);
}
