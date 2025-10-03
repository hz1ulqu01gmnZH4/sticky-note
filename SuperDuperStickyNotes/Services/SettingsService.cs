using System;
using System.IO;
using Newtonsoft.Json;
using SuperDuperStickyNotes.Models;

namespace SuperDuperStickyNotes.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;

        public AppSettings Settings => _settings;

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SuperDuperStickyNotes"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            _settings = LoadSettings();
        }

        private AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with defaults
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new AppSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }

        public void UpdateSettings(AppSettings newSettings)
        {
            _settings = newSettings;
            SaveSettings();
        }

        public void ResetToDefaults()
        {
            _settings = new AppSettings();
            SaveSettings();
        }
    }
}
