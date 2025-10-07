using Baluma.Emblue.ApiConsumer.Application.Reports.UseCases;
using Microsoft.Extensions.Logging;

namespace Baluma.Emblue.ApiConsumer.App.Presentation;

public partial class MainForm : Form
{
    private readonly IProcessDailyReportUseCase _processDailyReportUseCase;
    private readonly ILogger<MainForm> _logger;

    public MainForm(IProcessDailyReportUseCase processDailyReportUseCase, ILogger<MainForm> logger)
    {
        _processDailyReportUseCase = processDailyReportUseCase;
        _logger = logger;
        InitializeComponent();
    }

    private async void ProcessButton_Click(object sender, EventArgs e)
    {
        await RunDailyReportAsync();
    }

    private async Task RunDailyReportAsync()
    {
        try
        {
            ToggleProcessing(true);
            statusLabel.Text = "Procesando reporte diario...";
            DateOnly? selectedDate = datePicker.Checked
                ? DateOnly.FromDateTime(datePicker.Value.Date)
                : null;
            await _processDailyReportUseCase.ExecuteAsync(selectedDate, isAutomaticExecution: false, CancellationToken.None);
            statusLabel.Text = "Proceso finalizado correctamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el reporte diario");
            statusLabel.Text = $"Error: {ex.Message}";
            MessageBox.Show(this, ex.Message, "Error al procesar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleProcessing(false);
        }
    }

    private void ToggleProcessing(bool isProcessing)
    {
        processButton.Enabled = !isProcessing;
        datePicker.Enabled = !isProcessing;
    }
}
