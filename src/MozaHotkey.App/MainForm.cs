using MozaHotkey.Core;
using MozaHotkey.Core.Actions;
using MozaHotkey.Core.Settings;
using CoreKeys = MozaHotkey.Core.Settings.Keys;

namespace MozaHotkey.App;

public partial class MainForm : Form
{
    private readonly MozaDevice _device;
    private readonly AppSettings _settings;
    private GlobalHotkeyManager? _hotkeyManager;
    private NotifyIcon? _trayIcon;
    private bool _deviceConnected;

    public MainForm()
    {
        InitializeComponent();

        _device = new MozaDevice();
        _settings = AppSettings.Load();

        SetupTrayIcon();
        PopulateActionsList();
        LoadBindings();

        this.FormClosing += MainForm_FormClosing;
        this.Resize += MainForm_Resize;
        this.Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        // Set up hotkey manager after window handle is created
        _hotkeyManager = new GlobalHotkeyManager(this.Handle);
        _hotkeyManager.HotkeyTriggered += OnHotkeyTriggered;

        // Try to connect to device
        ConnectDevice();

        // Register hotkeys
        RegisterAllHotkeys();

        // Start minimized if configured
        if (_settings.StartMinimized)
        {
            MinimizeToTray();
        }
    }

    private void ConnectDevice()
    {
        try
        {
            _deviceConnected = _device.Initialize();
            UpdateConnectionStatus();
        }
        catch (Exception ex)
        {
            _deviceConnected = false;
            UpdateConnectionStatus();
            ShowNotification("Connection Failed", $"Could not connect to Moza device: {ex.Message}", ToolTipIcon.Error);
        }
    }

    private void UpdateConnectionStatus()
    {
        if (lblStatus != null)
        {
            lblStatus.Text = _deviceConnected ? "Connected" : "Not Connected";
            lblStatus.ForeColor = _deviceConnected ? Color.Green : Color.Red;
        }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Text = "MozaHotkey",
            Visible = true
        };

        // Create a simple icon programmatically
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.DodgerBlue);
        g.DrawString("M", new Font("Arial", 10, FontStyle.Bold), Brushes.White, -1, 0);
        _trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => ShowFromTray());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Reconnect Device", null, (s, e) => ConnectDevice());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApplication());

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (s, e) => ShowFromTray();
    }

    private void PopulateActionsList()
    {
        lstActions.Items.Clear();
        lstActions.View = View.Details;
        lstActions.FullRowSelect = true;
        lstActions.GridLines = true;

        lstActions.Columns.Clear();
        lstActions.Columns.Add("Action", 200);
        lstActions.Columns.Add("Hotkey", 150);
        lstActions.Columns.Add("Description", 250);

        foreach (var action in ActionRegistry.GetAllActions())
        {
            var item = new ListViewItem(action.Name);
            item.SubItems.Add("(Not Set)");
            item.SubItems.Add(action.Description);
            item.Tag = action;
            lstActions.Items.Add(item);
        }
    }

    private void LoadBindings()
    {
        foreach (ListViewItem item in lstActions.Items)
        {
            if (item.Tag is MozaAction action)
            {
                var binding = _settings.GetOrCreateBinding(action.Id);
                item.SubItems[1].Text = binding.DisplayString;
            }
        }
    }

    private void RegisterAllHotkeys()
    {
        _hotkeyManager?.UnregisterAll();

        foreach (var binding in _settings.Bindings.Where(b => b.HasHotkey))
        {
            var success = _hotkeyManager?.Register(binding) ?? false;
            if (!success)
            {
                ShowNotification("Hotkey Error", $"Could not register hotkey: {binding.DisplayString}", ToolTipIcon.Warning);
            }
        }
    }

    private void OnHotkeyTriggered(HotkeyBinding binding)
    {
        var action = ActionRegistry.GetAction(binding.ActionId);
        if (action == null) return;

        if (!_deviceConnected)
        {
            ShowNotification("Not Connected", "Moza device is not connected", ToolTipIcon.Warning);
            return;
        }

        try
        {
            var result = action.Execute(_device);
            ShowNotification(action.Name, result, ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            ShowNotification("Error", $"{action.Name} failed: {ex.Message}", ToolTipIcon.Error);
        }
    }

    private void ShowNotification(string title, string text, ToolTipIcon icon)
    {
        if (_settings.ShowNotifications && _trayIcon != null)
        {
            _trayIcon.ShowBalloonTip(2000, title, text, icon);
        }
    }

    private void lstActions_DoubleClick(object? sender, EventArgs e)
    {
        if (lstActions.SelectedItems.Count == 0) return;

        var item = lstActions.SelectedItems[0];
        if (item.Tag is not MozaAction action) return;

        using var dialog = new HotkeyDialog(_settings.GetOrCreateBinding(action.Id), action.Name);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _settings.Save();
            LoadBindings();
            RegisterAllHotkeys();
        }
    }

    private void btnSetHotkey_Click(object? sender, EventArgs e)
    {
        lstActions_DoubleClick(sender, e);
    }

    private void btnClearHotkey_Click(object? sender, EventArgs e)
    {
        if (lstActions.SelectedItems.Count == 0) return;

        var item = lstActions.SelectedItems[0];
        if (item.Tag is not MozaAction action) return;

        var binding = _settings.GetOrCreateBinding(action.Id);
        binding.Key = CoreKeys.None;
        binding.Ctrl = false;
        binding.Alt = false;
        binding.Shift = false;

        _settings.Save();
        LoadBindings();
        RegisterAllHotkeys();
    }

    private void btnTestAction_Click(object? sender, EventArgs e)
    {
        if (lstActions.SelectedItems.Count == 0) return;

        var item = lstActions.SelectedItems[0];
        if (item.Tag is not MozaAction action) return;

        if (!_deviceConnected)
        {
            MessageBox.Show("Moza device is not connected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var result = action.Execute(_device);
            MessageBox.Show(result, action.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", action.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void chkStartWithWindows_CheckedChanged(object? sender, EventArgs e)
    {
        _settings.StartWithWindows = chkStartWithWindows.Checked;
        _settings.Save();
        UpdateStartupRegistry();
    }

    private void chkStartMinimized_CheckedChanged(object? sender, EventArgs e)
    {
        _settings.StartMinimized = chkStartMinimized.Checked;
        _settings.Save();
    }

    private void chkShowNotifications_CheckedChanged(object? sender, EventArgs e)
    {
        _settings.ShowNotifications = chkShowNotifications.Checked;
        _settings.Save();
    }

    private void UpdateStartupRegistry()
    {
        var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (key == null) return;

        if (_settings.StartWithWindows)
        {
            key.SetValue("MozaHotkey", $"\"{Application.ExecutablePath}\"");
        }
        else
        {
            key.DeleteValue("MozaHotkey", false);
        }
    }

    private void MinimizeToTray()
    {
        this.Hide();
        this.ShowInTaskbar = false;
    }

    private void ShowFromTray()
    {
        this.Show();
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            MinimizeToTray();
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            MinimizeToTray();
        }
        else
        {
            ExitApplication();
        }
    }

    private void ExitApplication()
    {
        _hotkeyManager?.Dispose();
        _trayIcon?.Dispose();
        _device.Dispose();
        Application.Exit();
    }

    protected override void WndProc(ref Message m)
    {
        _hotkeyManager?.ProcessMessage(ref m);
        base.WndProc(ref m);
    }
}
