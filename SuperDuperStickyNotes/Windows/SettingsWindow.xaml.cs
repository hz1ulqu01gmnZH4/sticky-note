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

        public SettingsWindow(DatabaseContext database)
        {
            InitializeComponent();
            _database = database;
            _exportService = new ExportService(_database);

            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load settings from config or defaults
            // This is a placeholder - implement actual settings persistence later
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save settings
            // Placeholder for actual implementation
            MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
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