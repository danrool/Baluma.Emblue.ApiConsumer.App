namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
