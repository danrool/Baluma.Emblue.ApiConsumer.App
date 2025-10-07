using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using System.Windows.Forms;

namespace Baluma.Emblue.ApiConsumer.App.Presentation.Services;

public sealed class DesktopDuplicateDataHandler : IDuplicateDataHandler
{
    public Task<bool> ConfirmReplacementAsync(DateOnly date, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(cancellationToken);
        }

        var message = $"Ya existen datos para la fecha {date:yyyy-MM-dd}. ¿Desea reemplazarlos?";
        var result = MessageBox.Show(
            message,
            "Confirmar reemplazo",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        return Task.FromResult(result == DialogResult.Yes);
    }
}
