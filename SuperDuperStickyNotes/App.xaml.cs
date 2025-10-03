using System;
using System.Windows;
using System.Windows.Input;
using SuperDuperStickyNotes.Services;
using SuperDuperStickyNotes.Data;
using SuperDuperStickyNotes.Windows;

namespace SuperDuperStickyNotes
{
    public partial class App : Application
    {
        private HotkeyManager? _hotkeyManager;
        private TrayIconManager? _trayManager;
        private DatabaseContext? _database;
        private NoteManager? _noteManager;
        private CommandPaletteWindow? _commandPalette;
        private SettingsService? _settingsService;
        private BackupService? _backupService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize settings service
            _settingsService = new SettingsService();

            // Initialize database
            _database = new DatabaseContext();

            // Initialize backup service
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SuperDuperStickyNotes",
                "notes.db"
            );
            _backupService = new BackupService(_settingsService, dbPath);

            // Initialize note manager
            _noteManager = new NoteManager(_database);

            // Initialize system tray
            _trayManager = new TrayIconManager();
            _trayManager.NewNoteRequested += OnNewNoteRequested;
            _trayManager.ShowAllRequested += OnShowAllRequested;
            _trayManager.HideAllRequested += OnHideAllRequested;
            _trayManager.SettingsRequested += OnSettingsRequested;
            _trayManager.ExitRequested += OnExitRequested;

            // Initialize command palette
            _commandPalette = new CommandPaletteWindow(_noteManager);

            // Initialize global hotkeys
            _hotkeyManager = new HotkeyManager();
            _hotkeyManager.RegisterHotkey(ModifierKeys.Control, Key.Space, OnToggleCommandPalette);
            _hotkeyManager.RegisterHotkey(ModifierKeys.Control | ModifierKeys.Shift, Key.N, OnNewNoteRequested);
            _hotkeyManager.RegisterHotkey(ModifierKeys.Control | ModifierKeys.Shift, Key.F, OnSearchRequested);

            // Hide main window since we use individual note windows
            if (MainWindow != null)
            {
                MainWindow.Hide();
            }

            // Don't automatically load notes on startup
            // Users can open notes via tray menu or command palette
        }

        private void OnToggleCommandPalette()
        {
            if (_commandPalette != null)
            {
                if (_commandPalette.IsVisible)
                {
                    _commandPalette.Hide();
                }
                else
                {
                    _commandPalette.Show();
                    _commandPalette.Activate();
                    _commandPalette.FocusInput();
                }
            }
        }

        private void OnNewNoteRequested()
        {
            _noteManager?.CreateNewNote();
        }

        private void OnShowAllRequested()
        {
            _noteManager?.ShowAllNotes();
        }

        private void OnHideAllRequested()
        {
            _noteManager?.HideAllNotes();
        }

        private void OnSearchRequested()
        {
            // Open command palette in search mode
            if (_commandPalette != null)
            {
                if (!_commandPalette.IsVisible)
                {
                    _commandPalette.Show();
                    _commandPalette.Activate();
                }
                _commandPalette.FocusInput();
                _commandPalette.SetSearchMode();
            }
        }

        private void OnSettingsRequested()
        {
            if (_database != null && _settingsService != null)
            {
                var settingsWindow = new SettingsWindow(_database, _settingsService);
                settingsWindow.ShowDialog();
            }
        }

        private void OnExitRequested()
        {
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyManager?.Dispose();
            _trayManager?.Dispose();
            _backupService?.Dispose();
            _database?.Dispose();
            _noteManager?.Dispose();

            base.OnExit(e);
        }
    }
}