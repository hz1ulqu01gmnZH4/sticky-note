using System;
using System.IO;
using System.IO.Compression;
using System.Timers;

namespace SuperDuperStickyNotes.Services
{
    public class BackupService : IDisposable
    {
        private readonly SettingsService _settingsService;
        private readonly string _databasePath;
        private readonly Timer _backupTimer;
        private bool _disposed = false;

        public BackupService(SettingsService settingsService, string databasePath)
        {
            _settingsService = settingsService;
            _databasePath = databasePath;

            // Initialize timer for automatic backups
            _backupTimer = new Timer();
            _backupTimer.Elapsed += OnBackupTimerElapsed;

            UpdateBackupSchedule();

            // Perform initial backup on startup
            if (_settingsService.Settings.AutoBackupEnabled)
            {
                PerformBackup();
            }
        }

        public void UpdateBackupSchedule()
        {
            var settings = _settingsService.Settings;

            if (settings.AutoBackupEnabled && settings.AutoBackupIntervalHours > 0)
            {
                // Convert hours to milliseconds
                _backupTimer.Interval = settings.AutoBackupIntervalHours * 60 * 60 * 1000;
                _backupTimer.Start();
            }
            else
            {
                _backupTimer.Stop();
            }
        }

        private void OnBackupTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            PerformBackup();
        }

        public void PerformBackup()
        {
            try
            {
                var settings = _settingsService.Settings;

                if (!settings.AutoBackupEnabled)
                {
                    return;
                }

                // Ensure backup directory exists
                if (!Directory.Exists(settings.BackupDirectory))
                {
                    Directory.CreateDirectory(settings.BackupDirectory);
                }

                // Create backup filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"notes_backup_{timestamp}.db.gz";
                var backupFilePath = Path.Combine(settings.BackupDirectory, backupFileName);

                // Compress and copy database file
                if (File.Exists(_databasePath))
                {
                    using (FileStream sourceStream = new FileStream(_databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (FileStream destStream = new FileStream(backupFilePath, FileMode.Create))
                    using (GZipStream compressionStream = new GZipStream(destStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }

                    System.Diagnostics.Debug.WriteLine($"Backup created: {backupFilePath}");

                    // Clean up old backups
                    CleanupOldBackups();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup failed: {ex.Message}");
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var settings = _settingsService.Settings;
                var backupDirectory = settings.BackupDirectory;

                if (!Directory.Exists(backupDirectory))
                {
                    return;
                }

                var cutoffDate = DateTime.Now.AddDays(-settings.BackupRetentionDays);
                var backupFiles = Directory.GetFiles(backupDirectory, "notes_backup_*.db.gz");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"Deleted old backup: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Cleanup failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _backupTimer?.Stop();
                _backupTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}
