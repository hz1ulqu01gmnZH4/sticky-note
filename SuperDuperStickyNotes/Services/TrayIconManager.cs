using System;
using System.Drawing;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace SuperDuperStickyNotes.Services
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _contextMenu;

        public event Action? NewNoteRequested;
        public event Action? ShowAllRequested;
        public event Action? HideAllRequested;
        public event Action? SettingsRequested;
        public event Action? ExitRequested;

        public TrayIconManager()
        {
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();

            var newNoteItem = new ToolStripMenuItem("New Note");
            newNoteItem.Click += (s, e) => NewNoteRequested?.Invoke();

            var showAllItem = new ToolStripMenuItem("Show All Notes");
            showAllItem.Click += (s, e) => ShowAllRequested?.Invoke();

            var hideAllItem = new ToolStripMenuItem("Hide All Notes");
            hideAllItem.Click += (s, e) => HideAllRequested?.Invoke();

            var separator1 = new ToolStripSeparator();

            var settingsItem = new ToolStripMenuItem("Settings...");
            settingsItem.Click += (s, e) => SettingsRequested?.Invoke();

            var separator2 = new ToolStripSeparator();

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitRequested?.Invoke();

            _contextMenu.Items.Add(newNoteItem);
            _contextMenu.Items.Add(showAllItem);
            _contextMenu.Items.Add(hideAllItem);
            _contextMenu.Items.Add(separator1);
            _contextMenu.Items.Add(settingsItem);
            _contextMenu.Items.Add(separator2);
            _contextMenu.Items.Add(exitItem);

            // Create tray icon
            _trayIcon = new NotifyIcon
            {
                Icon = GetApplicationIcon(),
                Text = "Super Duper Sticky Notes",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _trayIcon.DoubleClick += (s, e) => NewNoteRequested?.Invoke();
        }

        private Icon GetApplicationIcon()
        {
            // Try to load icon from resources, otherwise use default
            try
            {
                // You would normally embed an icon resource here
                // For now, use a default system icon
                return SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _trayIcon.ShowBalloonTip(3000, title, text, icon);
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}