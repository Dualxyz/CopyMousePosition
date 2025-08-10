using System;
using System.Drawing;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GetMousePosition
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApp());
        }
    }

    public class TrayApp : ApplicationContext
    {
        private NotifyIcon trayIcon;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_Q = 0x51;

        public TrayApp()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Mouse Position Copier (Alt+Q)",
                Visible = true,
                ContextMenuStrip = BuildMenu()
            };

            bool registered = RegisterHotKey(IntPtr.Zero, HOTKEY_ID, MOD_ALT, VK_Q);

            if (!registered)
            {
                trayIcon.ShowBalloonTip(2000, "Warning", "Alt+Q is already in use by another app.", ToolTipIcon.Warning);
            }

            Application.AddMessageFilter(new HotkeyMessageFilter(OnHotkeyPressed));
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (s, e) => ExitApp());
            return menu;
        }

        private void OnHotkeyPressed()
        {
            var position = Cursor.Position;
            string coords = $"{position.X}, {position.Y}";
            Clipboard.SetText(coords);
        }

        private void ExitApp()
        {
            UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
            trayIcon.Visible = false;
            Application.Exit();
        }

        private class HotkeyMessageFilter : IMessageFilter
        {
            private readonly Action onHotkey;

            public HotkeyMessageFilter(Action onHotkey)
            {
                this.onHotkey = onHotkey;
            }

            public bool PreFilterMessage(ref Message m)
            {
                const int WM_HOTKEY = 0x0312;
                if (m.Msg == WM_HOTKEY)
                {
                    onHotkey();
                    return true;
                }
                return false;
            }
        }
    }
}
