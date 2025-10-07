using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.TaskExecution;
using Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.TaskExecution;

public sealed class TaskExecutionLogRepository : ITaskExecutionLogRepository
{
    private readonly ApiConsumerDbContext _dbContext;

    public TaskExecutionLogRepository(ApiConsumerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskExecutionLog> AddAsync(TaskExecutionLog log, CancellationToken cancellationToken)
    {
        await _dbContext.TaskExecutionLogs.AddAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return log;
    }

    public async Task UpdateAsync(TaskExecutionLog log, CancellationToken cancellationToken)
    {
        _dbContext.TaskExecutionLogs.Update(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
