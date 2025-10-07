using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Infrastructure.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Infrastructure.Configuration;
using Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;
using Baluma.Emblue.ApiConsumer.Infrastructure.TaskExecution;
using Baluma.Emblue.ApiConsumer.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AutomaticReportOptions>(configuration.GetSection(AutomaticReportOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddHttpClient<IAutomaticReportClient, EmblueAutomaticReportClient>();

        services.AddDbContext<ApiConsumerDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlite(databaseOptions.ConnectionString);
        });

        services.AddScoped<IDailyReportRepository, DailyReportRepository>();
        services.AddScoped<ITaskExecutionLogRepository, TaskExecutionLogRepository>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
