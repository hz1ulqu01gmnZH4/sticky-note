using FluentAssertions;
using SuperDuperStickyNotes.Data;
using SuperDuperStickyNotes.Models;
using System;
using System.IO;
using Xunit;

namespace SuperDuperStickyNotes.Tests.Data
{
    public class DatabaseContextTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseContext _database;

        public DatabaseContextTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            Environment.SetEnvironmentVariable("TEST_DB_PATH", _testDbPath);
            _database = CreateTestDatabase();
        }

        private DatabaseContext CreateTestDatabase()
        {
            // Create a temporary test database
            return new TestDatabaseContext(_testDbPath);
        }

        [Fact]
        public void CreateNote_ValidNote_ReturnsNoteWithSameId()
        {
            // Arrange
            var note = new Note("Test Content");

            // Act
            var result = _database.CreateNote(note);

            // Assert
            result.Id.Should().Be(note.Id);
            result.Content.Should().Be(note.Content);
        }

        [Fact]
        public void GetNote_ExistingId_ReturnsNote()
        {
            // Arrange
            var note = new Note("Test Content");
            _database.CreateNote(note);

            // Act
            var result = _database.GetNote(note.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(note.Id);
            result.Content.Should().Be("Test Content");
        }

        [Fact]
        public void GetNote_NonExistentId_ReturnsNull()
        {
            // Act
            var result = _database.GetNote("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAllNotes_WithMultipleNotes_ReturnsAllNotes()
        {
            // Arrange
            var note1 = new Note("Note 1");
            var note2 = new Note("Note 2");
            _database.CreateNote(note1);
            _database.CreateNote(note2);

            // Act
            var results = _database.GetAllNotes();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(n => n.Content == "Note 1");
            results.Should().Contain(n => n.Content == "Note 2");
        }

        [Fact]
        public void UpdateNote_ExistingNote_UpdatesContent()
        {
            // Arrange
            var note = new Note("Original Content");
            _database.CreateNote(note);
            note.Content = "Updated Content";

            // Act
            _database.UpdateNote(note);
            var result = _database.GetNote(note.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Content.Should().Be("Updated Content");
        }

        [Fact]
        public void DeleteNote_ExistingNote_RemovesNote()
        {
            // Arrange
            var note = new Note("Test Content");
            _database.CreateNote(note);

            // Act
            _database.DeleteNote(note.Id);
            var result = _database.GetNote(note.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void SearchNotes_MatchingContent_ReturnsMatchingNotes()
        {
            // Arrange
            var note1 = new Note("Important meeting notes");
            var note2 = new Note("Shopping list");
            var note3 = new Note("Meeting agenda");
            _database.CreateNote(note1);
            _database.CreateNote(note2);
            _database.CreateNote(note3);

            // Act
            var results = _database.SearchNotes("meeting");

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(n => n.Content.Contains("meeting", StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            _database?.Dispose();
            if (File.Exists(_testDbPath))
            {
                File.Delete(_testDbPath);
            }
        }
    }

    // Helper class for testing
    public class TestDatabaseContext : DatabaseContext
    {
        public TestDatabaseContext(string testPath) : base(testPath)
        {
        }
    }
}