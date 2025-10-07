namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IDuplicateDataHandler
{
    Task<bool> ConfirmReplacementAsync(DateOnly date, CancellationToken cancellationToken);
}
