using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SuperDuperStickyNotes.Data;
using SuperDuperStickyNotes.Models;

namespace SuperDuperStickyNotes.Services
{
    public enum ExportFormat
    {
        Json,
        Markdown,
        Csv
    }

    public class ExportService
    {
        private readonly DatabaseContext _database;

        public ExportService(DatabaseContext database)
        {
            _database = database;
        }

        public void ExportNotes(string filePath, ExportFormat format)
        {
            var notes = _database.GetAllNotes();

            switch (format)
            {
                case ExportFormat.Json:
                    ExportToJson(filePath, notes);
                    break;
                case ExportFormat.Markdown:
                    ExportToMarkdown(filePath, notes);
                    break;
                case ExportFormat.Csv:
                    ExportToCsv(filePath, notes);
                    break;
            }
        }

        private void ExportToJson(string filePath, List<Note> notes)
        {
            var export = new
            {
                version = "1.0",
                exported_at = DateTime.UtcNow,
                note_count = notes.Count,
                notes = notes
            };

            var json = JsonConvert.SerializeObject(export, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void ExportToMarkdown(string filePath, List<Note> notes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Super Duper Sticky Notes Export");
            sb.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Total Notes: {notes.Count}");
            sb.AppendLine();

            foreach (var note in notes)
            {
                sb.AppendLine("---");
                sb.AppendLine();
                sb.AppendLine($"## {note.Title}");
                sb.AppendLine();
                sb.AppendLine($"**ID:** {note.Id}");
                sb.AppendLine($"**Color:** {note.Color}");
                sb.AppendLine($"**Created:** {note.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"**Updated:** {note.UpdatedAt:yyyy-MM-dd HH:mm:ss}");

                if (note.Tags?.Any() == true)
                {
                    sb.AppendLine($"**Tags:** {string.Join(", ", note.Tags)}");
                }

                sb.AppendLine();
                sb.AppendLine("### Content");
                sb.AppendLine();
                sb.AppendLine(note.Content);
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private void ExportToCsv(string filePath, List<Note> notes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ID,Title,Content,Color,CreatedAt,UpdatedAt,Tags");

            foreach (var note in notes)
            {
                var content = EscapeCsv(note.Content);
                var title = EscapeCsv(note.Title);
                var tags = note.Tags != null ? EscapeCsv(string.Join(";", note.Tags)) : "";

                sb.AppendLine($"{note.Id},{title},{content},{note.Color},{note.CreatedAt:O},{note.UpdatedAt:O},{tags}");
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        public void ImportFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var import = JsonConvert.DeserializeAnonymousType(json, new
            {
                version = "",
                exported_at = DateTime.MinValue,
                note_count = 0,
                notes = new List<Note>()
            });

            if (import?.notes != null)
            {
                foreach (var note in import.notes)
                {
                    // Check if note already exists
                    var existing = _database.GetNote(note.Id);
                    if (existing == null)
                    {
                        _database.CreateNote(note);
                    }
                }
            }
        }
    }
}