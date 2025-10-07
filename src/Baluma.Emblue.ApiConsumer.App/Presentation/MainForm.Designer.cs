namespace Baluma.Emblue.ApiConsumer.App.Presentation;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private DateTimePicker datePicker;
    private Button processButton;
    private Label instructionsLabel;
    private Label statusLabel;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        datePicker = new DateTimePicker();
        processButton = new Button();
        instructionsLabel = new Label();
        statusLabel = new Label();
        SuspendLayout();
        // 
        // datePicker
        // 
        datePicker.Checked = false;
        datePicker.Format = DateTimePickerFormat.Short;
        datePicker.Location = new Point(32, 64);
        datePicker.Name = "datePicker";
        datePicker.ShowCheckBox = true;
        datePicker.Size = new Size(200, 23);
        datePicker.TabIndex = 0;
        // 
        // processButton
        // 
        processButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        processButton.Location = new Point(256, 64);
        processButton.Name = "processButton";
        processButton.Size = new Size(180, 23);
        processButton.TabIndex = 1;
        processButton.Text = "Procesar reporte diario";
        processButton.UseVisualStyleBackColor = true;
        processButton.Click += ProcessButton_Click;
        // 
        // instructionsLabel
        // 
        instructionsLabel.AutoSize = true;
        instructionsLabel.Location = new Point(32, 32);
        instructionsLabel.Name = "instructionsLabel";
        instructionsLabel.Size = new Size(436, 15);
        instructionsLabel.TabIndex = 2;
        instructionsLabel.Text = "Seleccione una fecha (opcional) y presione el botón para ejecutar el reporte diario.";
        // 
        // statusLabel
        // 
        statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        statusLabel.AutoEllipsis = true;
        statusLabel.Location = new Point(32, 104);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(404, 23);
        statusLabel.TabIndex = 3;
        statusLabel.Text = "Esperando acción.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(480, 160);
        Controls.Add(statusLabel);
        Controls.Add(instructionsLabel);
        Controls.Add(processButton);
        Controls.Add(datePicker);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Baluma Emblue - Procesos";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
