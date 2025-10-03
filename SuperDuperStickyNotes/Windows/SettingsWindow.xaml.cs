using System;
using System.Windows;
using Microsoft.Win32;
using SuperDuperStickyNotes.Data;
using SuperDuperStickyNotes.Services;

namespace SuperDuperStickyNotes.Windows
{
    public partial class SettingsWindow : Window
    {
        private readonly DatabaseContext _database;
        private readonly ExportService _exportService;
        private readonly SettingsService _settingsService;

        public SettingsWindow(DatabaseContext database, SettingsService settingsService)
        {
            InitializeComponent();
            _database = database;
            _exportService = new ExportService(_database);
            _settingsService = settingsService;

            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsService.Settings;

            // General Settings
            StartWithWindowsCheckBox.IsChecked = settings.StartWithWindows;
            StartMinimizedCheckBox.IsChecked = settings.StartMinimized;
            MinimizeToTrayCheckBox.IsChecked = settings.MinimizeToTray;
            AutoSaveIntervalTextBox.Text = settings.AutoSaveDelaySeconds.ToString();

            // Appearance Settings
            DefaultColorComboBox.SelectedIndex = GetColorIndex(settings.DefaultNoteColor);
            DefaultFontSizeComboBox.SelectedIndex = GetFontSizeIndex(settings.FontSize);
            DefaultWidthTextBox.Text = settings.DefaultNoteWidth.ToString();
            DefaultHeightTextBox.Text = settings.DefaultNoteHeight.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = _settingsService.Settings;

                // General Settings
                settings.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? false;
                settings.StartMinimized = StartMinimizedCheckBox.IsChecked ?? true;
                settings.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? true;

                if (int.TryParse(AutoSaveIntervalTextBox.Text, out int autoSaveInterval))
                {
                    settings.AutoSaveDelaySeconds = Math.Max(1, autoSaveInterval);
                }

                // Appearance Settings
                settings.DefaultNoteColor = GetColorHex(DefaultColorComboBox.SelectedIndex);
                settings.FontSize = GetFontSize(DefaultFontSizeComboBox.SelectedIndex);

                if (double.TryParse(DefaultWidthTextBox.Text, out double width))
                {
                    settings.DefaultNoteWidth = Math.Max(150, width);
                }

                if (double.TryParse(DefaultHeightTextBox.Text, out double height))
                {
                    settings.DefaultNoteHeight = Math.Max(100, height);
                }

                _settingsService.SaveSettings();

                MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetColorIndex(string colorHex)
        {
            return colorHex switch
            {
                "#FFEB3B" => 0, // Yellow
                "#4FC3F7" => 1, // Blue
                "#81C784" => 2, // Green
                "#F48FB1" => 3, // Pink
                "#CE93D8" => 4, // Purple
                "#FFAB91" => 5, // Orange
                "#EF5350" => 6, // Red
                "#B0BEC5" => 7, // Gray
                _ => 0
            };
        }

        private string GetColorHex(int index)
        {
            return index switch
            {
                0 => "#FFEB3B", // Yellow
                1 => "#4FC3F7", // Blue
                2 => "#81C784", // Green
                3 => "#F48FB1", // Pink
                4 => "#CE93D8", // Purple
                5 => "#FFAB91", // Orange
                6 => "#EF5350", // Red
                7 => "#B0BEC5", // Gray
                _ => "#FFEB3B"
            };
        }

        private int GetFontSizeIndex(int fontSize)
        {
            return fontSize switch
            {
                10 => 0,
                12 => 1,
                13 => 2,
                14 => 3,
                16 => 4,
                _ => 2 // Default to 13
            };
        }

        private int GetFontSize(int index)
        {
            return index switch
            {
                0 => 10,
                1 => 12,
                2 => 13,
                3 => 14,
                4 => 16,
                _ => 13
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = $"notes_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportNotes(dialog.FileName, ExportFormat.Json);
                    MessageBox.Show($"Notes exported successfully to:\n{dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting notes:\n{ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportMarkdown_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Markdown files (*.md)|*.md",
                FileName = $"notes_export_{DateTime.Now:yyyyMMdd_HHmmss}.md"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportNotes(dialog.FileName, ExportFormat.Markdown);
                    MessageBox.Show($"Notes exported successfully to:\n{dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting notes:\n{ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"notes_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ExportNotes(dialog.FileName, ExportFormat.Csv);
                    MessageBox.Show($"Notes exported successfully to:\n{dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting notes:\n{ex.Message}",
                        "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _exportService.ImportFromJson(dialog.FileName);
                    MessageBox.Show("Notes imported successfully!\n\nPlease restart the application to see imported notes.",
                        "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing notes:\n{ex.Message}",
                        "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}