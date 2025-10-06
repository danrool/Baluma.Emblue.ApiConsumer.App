using Baluma.Emblue.ApiConsumer.Domain.TaskExecution;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface ITaskExecutionLogRepository
{
    Task<TaskExecutionLog> AddAsync(TaskExecutionLog log, CancellationToken cancellationToken);
    Task UpdateAsync(TaskExecutionLog log, CancellationToken cancellationToken);
}
