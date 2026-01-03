namespace MozaHotkey.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.lstActions = new ListView();
        this.btnSetHotkey = new Button();
        this.btnClearHotkey = new Button();
        this.btnTestAction = new Button();
        this.grpSettings = new GroupBox();
        this.chkShowNotifications = new CheckBox();
        this.chkStartMinimized = new CheckBox();
        this.chkStartWithWindows = new CheckBox();
        this.lblStatusLabel = new Label();
        this.lblStatus = new Label();
        this.grpSettings.SuspendLayout();
        this.SuspendLayout();

        // lstActions
        this.lstActions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.lstActions.Location = new Point(12, 12);
        this.lstActions.Name = "lstActions";
        this.lstActions.Size = new Size(560, 280);
        this.lstActions.TabIndex = 0;
        this.lstActions.UseCompatibleStateImageBehavior = false;
        this.lstActions.DoubleClick += lstActions_DoubleClick;

        // btnSetHotkey
        this.btnSetHotkey.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.btnSetHotkey.Location = new Point(12, 298);
        this.btnSetHotkey.Name = "btnSetHotkey";
        this.btnSetHotkey.Size = new Size(100, 28);
        this.btnSetHotkey.TabIndex = 1;
        this.btnSetHotkey.Text = "Set Hotkey";
        this.btnSetHotkey.UseVisualStyleBackColor = true;
        this.btnSetHotkey.Click += btnSetHotkey_Click;

        // btnClearHotkey
        this.btnClearHotkey.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.btnClearHotkey.Location = new Point(118, 298);
        this.btnClearHotkey.Name = "btnClearHotkey";
        this.btnClearHotkey.Size = new Size(100, 28);
        this.btnClearHotkey.TabIndex = 2;
        this.btnClearHotkey.Text = "Clear Hotkey";
        this.btnClearHotkey.UseVisualStyleBackColor = true;
        this.btnClearHotkey.Click += btnClearHotkey_Click;

        // btnTestAction
        this.btnTestAction.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        this.btnTestAction.Location = new Point(224, 298);
        this.btnTestAction.Name = "btnTestAction";
        this.btnTestAction.Size = new Size(100, 28);
        this.btnTestAction.TabIndex = 3;
        this.btnTestAction.Text = "Test Action";
        this.btnTestAction.UseVisualStyleBackColor = true;
        this.btnTestAction.Click += btnTestAction_Click;

        // grpSettings
        this.grpSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        this.grpSettings.Controls.Add(this.chkShowNotifications);
        this.grpSettings.Controls.Add(this.chkStartMinimized);
        this.grpSettings.Controls.Add(this.chkStartWithWindows);
        this.grpSettings.Location = new Point(12, 335);
        this.grpSettings.Name = "grpSettings";
        this.grpSettings.Size = new Size(560, 55);
        this.grpSettings.TabIndex = 4;
        this.grpSettings.TabStop = false;
        this.grpSettings.Text = "Settings";

        // chkStartWithWindows
        this.chkStartWithWindows.AutoSize = true;
        this.chkStartWithWindows.Location = new Point(15, 24);
        this.chkStartWithWindows.Name = "chkStartWithWindows";
        this.chkStartWithWindows.Size = new Size(125, 19);
        this.chkStartWithWindows.TabIndex = 0;
        this.chkStartWithWindows.Text = "Start with Windows";
        this.chkStartWithWindows.UseVisualStyleBackColor = true;
        this.chkStartWithWindows.CheckedChanged += chkStartWithWindows_CheckedChanged;

        // chkStartMinimized
        this.chkStartMinimized.AutoSize = true;
        this.chkStartMinimized.Location = new Point(160, 24);
        this.chkStartMinimized.Name = "chkStartMinimized";
        this.chkStartMinimized.Size = new Size(110, 19);
        this.chkStartMinimized.TabIndex = 1;
        this.chkStartMinimized.Text = "Start Minimized";
        this.chkStartMinimized.UseVisualStyleBackColor = true;
        this.chkStartMinimized.CheckedChanged += chkStartMinimized_CheckedChanged;

        // chkShowNotifications
        this.chkShowNotifications.AutoSize = true;
        this.chkShowNotifications.Checked = true;
        this.chkShowNotifications.CheckState = CheckState.Checked;
        this.chkShowNotifications.Location = new Point(290, 24);
        this.chkShowNotifications.Name = "chkShowNotifications";
        this.chkShowNotifications.Size = new Size(125, 19);
        this.chkShowNotifications.TabIndex = 2;
        this.chkShowNotifications.Text = "Show Notifications";
        this.chkShowNotifications.UseVisualStyleBackColor = true;
        this.chkShowNotifications.CheckedChanged += chkShowNotifications_CheckedChanged;

        // lblStatusLabel
        this.lblStatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.lblStatusLabel.AutoSize = true;
        this.lblStatusLabel.Location = new Point(430, 304);
        this.lblStatusLabel.Name = "lblStatusLabel";
        this.lblStatusLabel.Size = new Size(45, 15);
        this.lblStatusLabel.TabIndex = 5;
        this.lblStatusLabel.Text = "Device:";

        // lblStatus
        this.lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        this.lblStatus.AutoSize = true;
        this.lblStatus.ForeColor = Color.Red;
        this.lblStatus.Location = new Point(480, 304);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new Size(85, 15);
        this.lblStatus.TabIndex = 6;
        this.lblStatus.Text = "Not Connected";

        // MainForm
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(584, 401);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.lblStatusLabel);
        this.Controls.Add(this.grpSettings);
        this.Controls.Add(this.btnTestAction);
        this.Controls.Add(this.btnClearHotkey);
        this.Controls.Add(this.btnSetHotkey);
        this.Controls.Add(this.lstActions);
        this.MinimumSize = new Size(500, 400);
        this.Name = "MainForm";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "MozaHotkey";
        this.grpSettings.ResumeLayout(false);
        this.grpSettings.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private ListView lstActions;
    private Button btnSetHotkey;
    private Button btnClearHotkey;
    private Button btnTestAction;
    private GroupBox grpSettings;
    private CheckBox chkShowNotifications;
    private CheckBox chkStartMinimized;
    private CheckBox chkStartWithWindows;
    private Label lblStatusLabel;
    private Label lblStatus;
}
