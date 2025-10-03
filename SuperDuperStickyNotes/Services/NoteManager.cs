using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SuperDuperStickyNotes.Data;
using SuperDuperStickyNotes.Models;
using SuperDuperStickyNotes.Windows;

namespace SuperDuperStickyNotes.Services
{
    public class NoteManager : IDisposable
    {
        private readonly DatabaseContext _database;
        private readonly Dictionary<string, NoteWindow> _noteWindows;
        private readonly ObservableCollection<Note> _notes;

        public ObservableCollection<Note> Notes => _notes;

        public NoteManager(DatabaseContext database)
        {
            _database = database;
            _noteWindows = new Dictionary<string, NoteWindow>();
            _notes = new ObservableCollection<Note>();

            LoadNotes();
        }

        private void LoadNotes()
        {
            var notes = _database.GetAllNotes();
            foreach (var note in notes)
            {
                _notes.Add(note);
            }
        }

        public Note CreateNewNote(string? content = null)
        {
            var note = new Note(content ?? string.Empty)
            {
                Position = GetNextNotePosition()
            };

            _database.CreateNote(note);
            _notes.Add(note);

            CreateNoteWindow(note);

            return note;
        }

        public void CreateNoteWindow(Note note)
        {
            if (_noteWindows.ContainsKey(note.Id))
            {
                // Window already exists, just focus it
                _noteWindows[note.Id].Focus();
                return;
            }

            var window = new NoteWindow(note);
            window.NoteUpdated += OnNoteUpdated;
            window.NoteDeleted += OnNoteDeleted;
            window.Closed += (s, e) => OnNoteWindowClosed(note.Id);

            _noteWindows[note.Id] = window;
            window.Show();
        }

        private void OnNoteUpdated(Note note)
        {
            _database.UpdateNote(note);
        }

        private void OnNoteDeleted(string noteId)
        {
            DeleteNote(noteId);
        }

        private void OnNoteWindowClosed(string noteId)
        {
            if (_noteWindows.ContainsKey(noteId))
            {
                _noteWindows.Remove(noteId);
            }
        }

        public void DeleteNote(string noteId)
        {
            _database.DeleteNote(noteId);

            var note = _notes.FirstOrDefault(n => n.Id == noteId);
            if (note != null)
            {
                _notes.Remove(note);
            }

            if (_noteWindows.ContainsKey(noteId))
            {
                _noteWindows[noteId].Close();
                _noteWindows.Remove(noteId);
            }
        }

        public void ShowAllNotes()
        {
            foreach (var window in _noteWindows.Values)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
            }

            // Also create windows for notes that don't have windows yet
            foreach (var note in _notes)
            {
                if (!_noteWindows.ContainsKey(note.Id))
                {
                    CreateNoteWindow(note);
                }
            }
        }

        public void HideAllNotes()
        {
            foreach (var window in _noteWindows.Values)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        public void FocusNote(string noteId)
        {
            if (_noteWindows.ContainsKey(noteId))
            {
                var window = _noteWindows[noteId];
                window.Show();
                window.WindowState = WindowState.Normal;
                window.Focus();
                window.Activate();
            }
            else
            {
                var note = _notes.FirstOrDefault(n => n.Id == noteId);
                if (note != null)
                {
                    CreateNoteWindow(note);
                }
            }
        }

        public List<Note> SearchNotes(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _notes.ToList();
            }

            // First try database full-text search
            var searchResults = _database.SearchNotes(query);
            if (searchResults.Any())
            {
                return searchResults;
            }

            // Fallback to in-memory search
            query = query.ToLower();
            return _notes
                .Where(n => n.Title.ToLower().Contains(query) ||
                           n.Content.ToLower().Contains(query))
                .ToList();
        }

        private Position GetNextNotePosition()
        {
            // Calculate a cascading position for new notes
            var openWindowsCount = _noteWindows.Count;
            var offset = openWindowsCount * 30;

            return new Position
            {
                X = 100 + offset,
                Y = 100 + offset
            };
        }

        public void Dispose()
        {
            foreach (var window in _noteWindows.Values)
            {
                window.Close();
            }
            _noteWindows.Clear();
        }
    }
}