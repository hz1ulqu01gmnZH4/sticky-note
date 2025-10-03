using System;

namespace SuperDuperStickyNotes.Models
{
    public class AppSettings
    {
        // General Settings
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimized { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public bool ShowInTaskbar { get; set; } = false;

        // Appearance Settings
        public string DefaultNoteColor { get; set; } = "#FFEB3B"; // Yellow
        public string FontFamily { get; set; } = "Segoe UI";
        public int FontSize { get; set; } = 13;
        public double DefaultNoteWidth { get; set; } = 250;
        public double DefaultNoteHeight { get; set; } = 300;
        public double NoteOpacity { get; set; } = 1.0;

        // Backup Settings
        public bool AutoBackupEnabled { get; set; } = true;
        public int AutoBackupIntervalHours { get; set; } = 24;
        public int BackupRetentionDays { get; set; } = 7;
        public string BackupDirectory { get; set; } = string.Empty;

        // Advanced Settings
        public bool EnableSpellCheck { get; set; } = true;
        public int AutoSaveDelaySeconds { get; set; } = 2;

        public AppSettings()
        {
            // Set default backup directory to AppData
            BackupDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SuperDuperStickyNotes",
                "Backups"
            );
        }
    }
}
