using System.Runtime.InteropServices;
using MozaHotkey.Core.Settings;
using Keys = MozaHotkey.Core.Settings.Keys;
using WinKeys = System.Windows.Forms.Keys;

namespace MozaHotkey.App;

/// <summary>
/// Dialog for capturing and setting a hotkey with conflict detection.
/// </summary>
public class HotkeyDialog : Form
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int TEST_HOTKEY_ID = 9999;

    private readonly HotkeyBinding _binding;
    private readonly Label lblInstruction;
    private readonly Label lblCurrentHotkey;
    private readonly Label lblStatus;
    private readonly Label lblGuidelines;
    private readonly Button btnOk;
    private readonly Button btnCancel;
    private readonly Button btnClear;

    private Keys _capturedKey = Keys.None;
    private bool _capturedCtrl;
    private bool _capturedAlt;
    private bool _capturedShift;
    private bool _isAvailable = true;

    public HotkeyDialog(HotkeyBinding binding, string actionName)
    {
        _binding = binding;

        this.Text = $"Set Hotkey - {actionName}";
        this.Size = new Size(400, 280);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.KeyPreview = true;

        lblInstruction = new Label
        {
            Text = "Press the key combination you want to use:",
            Location = new Point(12, 15),
            Size = new Size(370, 20)
        };

        lblCurrentHotkey = new Label
        {
            Text = binding.DisplayString,
            Location = new Point(12, 40),
            Size = new Size(370, 35),
            Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

        lblStatus = new Label
        {
            Text = "",
            Location = new Point(12, 80),
            Size = new Size(370, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };

        lblGuidelines = new Label
        {
            Text = "Tips for safe hotkeys:\n" +
                   "• Use Ctrl+Alt+Key or Ctrl+Shift+Key combinations\n" +
                   "• Avoid common shortcuts (Ctrl+C, Ctrl+V, Alt+Tab, etc.)\n" +
                   "• Function keys (F1-F12) with modifiers are usually safe\n" +
                   "• Numpad keys with modifiers work well for sim racing",
            Location = new Point(12, 105),
            Size = new Size(370, 80),
            ForeColor = Color.DimGray
        };

        btnOk = new Button
        {
            Text = "OK",
            Location = new Point(115, 195),
            Size = new Size(80, 30)
        };
        btnOk.Click += BtnOk_Click;

        btnClear = new Button
        {
            Text = "Clear",
            Location = new Point(205, 195),
            Size = new Size(80, 30)
        };
        btnClear.Click += (s, e) =>
        {
            _capturedKey = Keys.None;
            _capturedCtrl = false;
            _capturedAlt = false;
            _capturedShift = false;
            _isAvailable = true;
            UpdateDisplay();
        };

        btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(295, 195),
            Size = new Size(80, 30)
        };

        this.Controls.Add(lblInstruction);
        this.Controls.Add(lblCurrentHotkey);
        this.Controls.Add(lblStatus);
        this.Controls.Add(lblGuidelines);
        this.Controls.Add(btnOk);
        this.Controls.Add(btnClear);
        this.Controls.Add(btnCancel);

        this.CancelButton = btnCancel;

        // Initialize with current binding
        _capturedKey = binding.Key;
        _capturedCtrl = binding.Ctrl;
        _capturedAlt = binding.Alt;
        _capturedShift = binding.Shift;

        this.KeyDown += HotkeyDialog_KeyDown;
        this.Load += (s, e) => UpdateDisplay();
    }

    private void HotkeyDialog_KeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        e.SuppressKeyPress = true;

        // Ignore modifier-only keys
        if (e.KeyCode == WinKeys.ControlKey || e.KeyCode == WinKeys.ShiftKey ||
            e.KeyCode == WinKeys.Menu || e.KeyCode == WinKeys.LMenu ||
            e.KeyCode == WinKeys.RMenu || e.KeyCode == WinKeys.LControlKey ||
            e.KeyCode == WinKeys.RControlKey || e.KeyCode == WinKeys.LShiftKey ||
            e.KeyCode == WinKeys.RShiftKey)
        {
            return;
        }

        // Require at least one modifier for most keys (except F1-F12)
        bool isFunctionKey = e.KeyCode >= WinKeys.F1 && e.KeyCode <= WinKeys.F12;
        if (!e.Control && !e.Alt && !e.Shift && !isFunctionKey)
        {
            lblStatus.Text = "Please use at least one modifier (Ctrl, Alt, or Shift)";
            lblStatus.ForeColor = Color.Orange;
            return;
        }

        _capturedCtrl = e.Control;
        _capturedAlt = e.Alt;
        _capturedShift = e.Shift;
        _capturedKey = ConvertKey(e.KeyCode);

        UpdateDisplay();
    }

    private static Keys ConvertKey(WinKeys key)
    {
        if (Enum.IsDefined(typeof(Keys), (int)key))
        {
            return (Keys)(int)key;
        }
        return Keys.None;
    }

    private void UpdateDisplay()
    {
        if (_capturedKey == Keys.None)
        {
            lblCurrentHotkey.Text = "(Not Set)";
            lblCurrentHotkey.BackColor = Color.White;
            lblStatus.Text = "";
            btnOk.Enabled = true;
            _isAvailable = true;
            return;
        }

        // Build display string
        var parts = new List<string>();
        if (_capturedCtrl) parts.Add("Ctrl");
        if (_capturedAlt) parts.Add("Alt");
        if (_capturedShift) parts.Add("Shift");
        parts.Add(_capturedKey.ToString());
        lblCurrentHotkey.Text = string.Join(" + ", parts);

        // Test if hotkey is available
        _isAvailable = TestHotkeyAvailability();

        if (_isAvailable)
        {
            lblCurrentHotkey.BackColor = Color.LightGreen;
            lblStatus.Text = "Hotkey is available";
            lblStatus.ForeColor = Color.Green;
            btnOk.Enabled = true;
        }
        else
        {
            lblCurrentHotkey.BackColor = Color.LightCoral;
            lblStatus.Text = "Hotkey is already in use by another application";
            lblStatus.ForeColor = Color.Red;
            btnOk.Enabled = false;
        }
    }

    private bool TestHotkeyAvailability()
    {
        if (_capturedKey == Keys.None) return true;

        // Calculate modifiers
        uint mods = 0;
        if (_capturedAlt) mods |= 0x0001;    // MOD_ALT
        if (_capturedCtrl) mods |= 0x0002;   // MOD_CONTROL
        if (_capturedShift) mods |= 0x0004;  // MOD_SHIFT

        // Try to register the hotkey
        bool success = RegisterHotKey(this.Handle, TEST_HOTKEY_ID, mods, (uint)_capturedKey);

        if (success)
        {
            // Immediately unregister - we just wanted to test
            UnregisterHotKey(this.Handle, TEST_HOTKEY_ID);
            return true;
        }

        return false;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (!_isAvailable && _capturedKey != Keys.None)
        {
            MessageBox.Show(
                "This hotkey is already in use by another application.\n\nPlease choose a different combination.",
                "Hotkey Unavailable",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _binding.Key = _capturedKey;
        _binding.Ctrl = _capturedCtrl;
        _binding.Alt = _capturedAlt;
        _binding.Shift = _capturedShift;

        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Make sure we clean up any test registration
        UnregisterHotKey(this.Handle, TEST_HOTKEY_ID);
        base.OnFormClosing(e);
    }
}
