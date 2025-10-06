using Baluma.Emblue.ApiConsumer.Application.Reports.Parsers;
using Baluma.Emblue.ApiConsumer.Application.Reports.UseCases;
using Baluma.Emblue.ApiConsumer.Infrastructure.Extensions;
using Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Baluma.Emblue.ApiConsumer.Presentation.WinForms;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureConfiguration(builder.Configuration, builder.Environment);
        ConfigureServices(builder.Services, builder.Configuration);

        await using var host = builder.Build();
        await EnsureDatabaseAsync(host.Services);

        if (await TryExecuteFromCommandLineAsync(args, host.Services))
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        using var scope = host.Services.CreateScope();
        var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }

    private static void ConfigureConfiguration(ConfigurationManager configuration, HostApplicationBuilderEnvironment environment)
    {
        configuration.Sources.Clear();
        configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.AddScoped<IReportContentParser, DailyActivityDetailReportParser>();
        services.AddScoped<IReportContentParser, DailyActionSummaryReportParser>();
        services.AddScoped<IProcessDailyReportUseCase, ProcessDailyReportUseCase>();
        services.AddScoped<MainForm>();
    }

    private static async Task EnsureDatabaseAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiConsumerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private static async Task<bool> TryExecuteFromCommandLineAsync(string[] args, IServiceProvider services)
    {
        if (args.Length == 0)
        {
            return false;
        }

        var taskName = args[0];
        if (taskName.Equals("ProcesarReporteDiario", StringComparison.OrdinalIgnoreCase))
        {
            DateOnly? date = null;
            if (args.Length > 1 && DateOnly.TryParse(args[1], out var parsedDate))
            {
                date = parsedDate;
            }

            await using var scope = services.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<IProcessDailyReportUseCase>();
            await useCase.ExecuteAsync(date, CancellationToken.None);
            Environment.ExitCode = 0;
            return true;
        }

        Console.Error.WriteLine($"Tarea desconocida: {taskName}");
        Environment.ExitCode = 1;
        return true;
    }
}
