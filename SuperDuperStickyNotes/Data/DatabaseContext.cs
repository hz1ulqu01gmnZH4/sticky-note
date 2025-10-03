using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SuperDuperStickyNotes.Models;

namespace SuperDuperStickyNotes.Data
{
    public class DatabaseContext : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly string _connectionString;

        public DatabaseContext() : this(GetDefaultDatabasePath())
        {
        }

        protected DatabaseContext(string dbPath)
        {
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _connectionString = $"Data Source={dbPath};Version=3;";

            _connection = new SQLiteConnection(_connectionString);
            _connection.Open();

            InitializeDatabase();
        }

        private static string GetDefaultDatabasePath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SuperDuperStickyNotes"
            );

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, "notes.db");
        }

        private void InitializeDatabase()
        {
            string createTableSql = @"
                CREATE TABLE IF NOT EXISTS notes (
                    id TEXT PRIMARY KEY NOT NULL,
                    content TEXT NOT NULL,
                    title TEXT NOT NULL,
                    color TEXT NOT NULL,
                    position_x REAL NOT NULL,
                    position_y REAL NOT NULL,
                    width REAL NOT NULL,
                    height REAL NOT NULL,
                    workspace_id TEXT NOT NULL,
                    group_id TEXT,
                    pinned INTEGER NOT NULL,
                    created_at TEXT NOT NULL,
                    updated_at TEXT NOT NULL,
                    metadata TEXT
                );

                CREATE INDEX IF NOT EXISTS idx_notes_workspace ON notes(workspace_id);
                CREATE INDEX IF NOT EXISTS idx_notes_group ON notes(group_id);
                CREATE INDEX IF NOT EXISTS idx_notes_updated ON notes(updated_at);

                CREATE VIRTUAL TABLE IF NOT EXISTS notes_fts USING fts4(
                    id,
                    content,
                    title,
                    tokenize=simple
                );

                CREATE TRIGGER IF NOT EXISTS notes_ai AFTER INSERT ON notes BEGIN
                    INSERT INTO notes_fts(id, content, title)
                    VALUES (new.id, new.content, new.title);
                END;

                CREATE TRIGGER IF NOT EXISTS notes_au AFTER UPDATE ON notes BEGIN
                    UPDATE notes_fts SET content = new.content, title = new.title
                    WHERE id = new.id;
                END;

                CREATE TRIGGER IF NOT EXISTS notes_ad AFTER DELETE ON notes BEGIN
                    DELETE FROM notes_fts WHERE id = old.id;
                END;
            ";

            using (var command = new SQLiteCommand(createTableSql, _connection))
            {
                command.ExecuteNonQuery();
            }

            // Migration: Add tags column if it doesn't exist
            string checkColumnSql = "PRAGMA table_info(notes)";
            bool hasTagsColumn = false;

            using (var command = new SQLiteCommand(checkColumnSql, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1) == "tags")
                        {
                            hasTagsColumn = true;
                            break;
                        }
                    }
                }
            }

            if (!hasTagsColumn)
            {
                string alterTableSql = "ALTER TABLE notes ADD COLUMN tags TEXT DEFAULT '[]'";
                using (var command = new SQLiteCommand(alterTableSql, _connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public Note CreateNote(Note note)
        {
            string sql = @"
                INSERT INTO notes (
                    id, content, title, color,
                    position_x, position_y, width, height,
                    workspace_id, group_id, pinned,
                    created_at, updated_at, metadata, tags
                ) VALUES (
                    @id, @content, @title, @color,
                    @posX, @posY, @width, @height,
                    @workspaceId, @groupId, @pinned,
                    @createdAt, @updatedAt, @metadata, @tags
                )";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@id", note.Id);
                command.Parameters.AddWithValue("@content", note.Content);
                command.Parameters.AddWithValue("@title", note.Title);
                command.Parameters.AddWithValue("@color", note.Color);
                command.Parameters.AddWithValue("@posX", note.Position.X);
                command.Parameters.AddWithValue("@posY", note.Position.Y);
                command.Parameters.AddWithValue("@width", note.Size.Width);
                command.Parameters.AddWithValue("@height", note.Size.Height);
                command.Parameters.AddWithValue("@workspaceId", note.WorkspaceId);
                command.Parameters.AddWithValue("@groupId", (object?)note.GroupId ?? DBNull.Value);
                command.Parameters.AddWithValue("@pinned", note.Pinned ? 1 : 0);
                command.Parameters.AddWithValue("@createdAt", note.CreatedAt.ToString("O"));
                command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("O"));
                command.Parameters.AddWithValue("@metadata", JsonConvert.SerializeObject(note.Metadata));
                command.Parameters.AddWithValue("@tags", JsonConvert.SerializeObject(note.Tags));

                command.ExecuteNonQuery();
            }

            return note;
        }

        public Note? GetNote(string id)
        {
            string sql = @"
                SELECT id, content, title, color,
                       position_x, position_y, width, height,
                       workspace_id, group_id, pinned,
                       created_at, updated_at, metadata, tags
                FROM notes WHERE id = @id";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadNoteFromReader(reader);
                    }
                }
            }

            return null;
        }

        public List<Note> GetAllNotes()
        {
            var notes = new List<Note>();
            string sql = @"
                SELECT id, content, title, color,
                       position_x, position_y, width, height,
                       workspace_id, group_id, pinned,
                       created_at, updated_at, metadata, tags
                FROM notes ORDER BY updated_at DESC";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notes.Add(ReadNoteFromReader(reader));
                    }
                }
            }

            return notes;
        }

        public List<Note> GetWorkspaceNotes(string workspaceId)
        {
            var notes = new List<Note>();
            string sql = @"
                SELECT id, content, title, color,
                       position_x, position_y, width, height,
                       workspace_id, group_id, pinned,
                       created_at, updated_at, metadata, tags
                FROM notes
                WHERE workspace_id = @workspaceId
                ORDER BY updated_at DESC";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@workspaceId", workspaceId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notes.Add(ReadNoteFromReader(reader));
                    }
                }
            }

            return notes;
        }

        public void UpdateNote(Note note)
        {
            string sql = @"
                UPDATE notes SET
                    content = @content,
                    title = @title,
                    color = @color,
                    position_x = @posX,
                    position_y = @posY,
                    width = @width,
                    height = @height,
                    workspace_id = @workspaceId,
                    group_id = @groupId,
                    pinned = @pinned,
                    updated_at = @updatedAt,
                    metadata = @metadata,
                    tags = @tags
                WHERE id = @id";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@id", note.Id);
                command.Parameters.AddWithValue("@content", note.Content);
                command.Parameters.AddWithValue("@title", note.Title);
                command.Parameters.AddWithValue("@color", note.Color);
                command.Parameters.AddWithValue("@posX", note.Position.X);
                command.Parameters.AddWithValue("@posY", note.Position.Y);
                command.Parameters.AddWithValue("@width", note.Size.Width);
                command.Parameters.AddWithValue("@height", note.Size.Height);
                command.Parameters.AddWithValue("@workspaceId", note.WorkspaceId);
                command.Parameters.AddWithValue("@groupId", (object?)note.GroupId ?? DBNull.Value);
                command.Parameters.AddWithValue("@pinned", note.Pinned ? 1 : 0);
                command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("O"));
                command.Parameters.AddWithValue("@metadata", JsonConvert.SerializeObject(note.Metadata));
                command.Parameters.AddWithValue("@tags", JsonConvert.SerializeObject(note.Tags));

                command.ExecuteNonQuery();
            }
        }

        public void DeleteNote(string id)
        {
            string sql = "DELETE FROM notes WHERE id = @id";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public List<Note> SearchNotes(string query)
        {
            var notes = new List<Note>();
            string sql = @"
                SELECT n.id, n.content, n.title, n.color,
                       n.position_x, n.position_y, n.width, n.height,
                       n.workspace_id, n.group_id, n.pinned,
                       n.created_at, n.updated_at, n.metadata, n.tags
                FROM notes n
                JOIN notes_fts f ON n.id = f.id
                WHERE notes_fts MATCH @query";

            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@query", query);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notes.Add(ReadNoteFromReader(reader));
                    }
                }
            }

            return notes;
        }

        private Note ReadNoteFromReader(SQLiteDataReader reader)
        {
            var note = new Note
            {
                Id = reader.GetString(0),
                Content = reader.GetString(1),
                Title = reader.GetString(2),
                Color = reader.GetString(3),
                Position = new Position
                {
                    X = reader.GetDouble(4),
                    Y = reader.GetDouble(5)
                },
                Size = new Size
                {
                    Width = reader.GetDouble(6),
                    Height = reader.GetDouble(7)
                },
                WorkspaceId = reader.GetString(8),
                GroupId = reader.IsDBNull(9) ? null : reader.GetString(9),
                Pinned = reader.GetInt32(10) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(11)),
                UpdatedAt = DateTime.Parse(reader.GetString(12)),
                Metadata = reader.IsDBNull(13)
                    ? new Dictionary<string, object>()
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.GetString(13))
                      ?? new Dictionary<string, object>()
            };

            // Deserialize tags (column 14)
            if (!reader.IsDBNull(14))
            {
                var tagsJson = reader.GetString(14);
                var tags = JsonConvert.DeserializeObject<List<string>>(tagsJson);
                if (tags != null)
                {
                    note.Tags.Clear();
                    foreach (var tag in tags)
                    {
                        note.Tags.Add(tag);
                    }
                }
            }

            return note;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}