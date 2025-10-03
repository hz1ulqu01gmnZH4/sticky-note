using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SuperDuperStickyNotes.Models;
using SuperDuperStickyNotes.Services;

namespace SuperDuperStickyNotes
{
    public partial class CommandPaletteWindow : Window
    {
        private readonly NoteManager _noteManager;
        private List<CommandSuggestion> _suggestions;

        public CommandPaletteWindow(NoteManager noteManager)
        {
            InitializeComponent();
            _noteManager = noteManager;
            _suggestions = new List<CommandSuggestion>();
        }

        public void FocusInput()
        {
            CommandInput.Focus();
            CommandInput.SelectAll();
        }

        public void SetSearchMode()
        {
            CommandInput.Text = "/";
            CommandInput.CaretIndex = 1;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
            SuggestionsPopup.IsOpen = false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                e.Handled = true;
            }
        }

        private void CommandInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSuggestions();
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteCommand();
                e.Handled = true;
            }
            else if (e.Key == Key.Down && SuggestionsPopup.IsOpen)
            {
                SuggestionsList.Focus();
                if (SuggestionsList.Items.Count > 0)
                {
                    SuggestionsList.SelectedIndex = 0;
                }
                e.Handled = true;
            }
        }

        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsList.SelectedItem is CommandSuggestion suggestion)
            {
                CommandInput.Text = suggestion.Command;
                CommandInput.CaretIndex = CommandInput.Text.Length;
                CommandInput.Focus();
            }
        }

        private void UpdateSuggestions()
        {
            var input = CommandInput.Text.Trim();
            _suggestions.Clear();

            if (string.IsNullOrEmpty(input))
            {
                // Show default suggestions
                _suggestions.Add(new CommandSuggestion
                {
                    Icon = "📝",
                    Title = "new",
                    Description = "Create a new note",
                    Command = "new "
                });
                _suggestions.Add(new CommandSuggestion
                {
                    Icon = "🔍",
                    Title = "/",
                    Description = "Search notes",
                    Command = "/"
                });
                _suggestions.Add(new CommandSuggestion
                {
                    Icon = "👁",
                    Title = "show",
                    Description = "Show all notes",
                    Command = "show"
                });
                _suggestions.Add(new CommandSuggestion
                {
                    Icon = "🔽",
                    Title = "hide",
                    Description = "Hide all notes",
                    Command = "hide"
                });
                _suggestions.Add(new CommandSuggestion
                {
                    Icon = "⚙",
                    Title = "!settings",
                    Description = "Open settings",
                    Command = "!settings"
                });
            }
            else if (input.StartsWith("/"))
            {
                // Search mode
                var query = input.Substring(1).Trim();
                if (!string.IsNullOrEmpty(query))
                {
                    var notes = _noteManager.SearchNotes(query);
                    foreach (var note in notes.Take(5))
                    {
                        _suggestions.Add(new CommandSuggestion
                        {
                            Icon = "📄",
                            Title = note.Title,
                            Description = $"Updated: {note.UpdatedAt:g}",
                            Command = $"@{note.Id}",
                            Data = note
                        });
                    }
                }
            }
            else if (input.StartsWith("new "))
            {
                // New note with content
                var content = input.Substring(4).Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    _suggestions.Add(new CommandSuggestion
                    {
                        Icon = "➕",
                        Title = "Create note",
                        Description = content.Length > 50 ? content.Substring(0, 50) + "..." : content,
                        Command = input
                    });
                }
            }
            else if (input.StartsWith("!"))
            {
                // System commands
                var commands = new[]
                {
                    ("!settings", "Open settings"),
                    ("!minimize", "Minimize all notes"),
                    ("!restore", "Restore all notes"),
                    ("!quit", "Exit application")
                };

                var cmd = input.Substring(1).ToLower();
                foreach (var (command, description) in commands)
                {
                    if (command.StartsWith("!" + cmd))
                    {
                        _suggestions.Add(new CommandSuggestion
                        {
                            Icon = "⚙",
                            Title = command,
                            Description = description,
                            Command = command
                        });
                    }
                }
            }
            else
            {
                // Default to creating a new note
                if (!string.IsNullOrWhiteSpace(input))
                {
                    _suggestions.Add(new CommandSuggestion
                    {
                        Icon = "➕",
                        Title = "Create note",
                        Description = input.Length > 50 ? input.Substring(0, 50) + "..." : input,
                        Command = "new " + input
                    });
                }
            }

            SuggestionsList.ItemsSource = _suggestions;
            SuggestionsPopup.IsOpen = _suggestions.Count > 0;
        }

        private void ExecuteCommand()
        {
            var input = CommandInput.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Hide();
                return;
            }

            // Parse and execute command
            if (input.StartsWith("new "))
            {
                var content = input.Substring(4).Trim();
                _noteManager.CreateNewNote(content);
            }
            else if (input.StartsWith("@"))
            {
                // Focus existing note
                var noteId = input.Substring(1).Trim();
                _noteManager.FocusNote(noteId);
            }
            else if (input == "show")
            {
                _noteManager.ShowAllNotes();
            }
            else if (input == "hide")
            {
                _noteManager.HideAllNotes();
            }
            else if (input == "!minimize")
            {
                _noteManager.HideAllNotes();
            }
            else if (input == "!restore")
            {
                _noteManager.ShowAllNotes();
            }
            else if (input == "!quit")
            {
                Application.Current.Shutdown();
            }
            else if (!input.StartsWith("/") && !input.StartsWith("!"))
            {
                // Default to creating a new note
                _noteManager.CreateNewNote(input);
            }

            // Clear and hide
            CommandInput.Clear();
            Hide();
        }

        private class CommandSuggestion
        {
            public string Icon { get; set; } = "";
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Command { get; set; } = "";
            public object? Data { get; set; }
        }
    }
}