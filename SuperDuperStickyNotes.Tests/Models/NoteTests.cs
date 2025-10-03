using FluentAssertions;
using SuperDuperStickyNotes.Models;
using Xunit;

namespace SuperDuperStickyNotes.Tests.Models
{
    public class NoteTests
    {
        [Fact]
        public void Constructor_WithoutContent_CreatesNoteWithDefaults()
        {
            // Act
            var note = new Note();

            // Assert
            note.Id.Should().NotBeNullOrEmpty();
            note.Content.Should().BeEmpty();
            note.Title.Should().Be("New Note");
            note.Color.Should().Be("#FFEB3B");
            note.Position.Should().NotBeNull();
            note.Position.X.Should().Be(100);
            note.Position.Y.Should().Be(100);
            note.Size.Width.Should().Be(250);
            note.Size.Height.Should().Be(300);
            note.Pinned.Should().BeFalse();
            note.WorkspaceId.Should().Be("default");
        }

        [Fact]
        public void Constructor_WithContent_SetsTitleFromFirstLine()
        {
            // Arrange
            var content = "My First Note\nThis is the body";

            // Act
            var note = new Note(content);

            // Assert
            note.Content.Should().Be(content);
            note.Title.Should().Be("My First Note");
        }

        [Fact]
        public void Constructor_WithLongContent_TruncatesTitleAt50Chars()
        {
            // Arrange
            var longTitle = new string('A', 60);
            var content = $"{longTitle}\nBody";

            // Act
            var note = new Note(content);

            // Assert
            note.Title.Length.Should().Be(50);
            note.Title.Should().Be(longTitle.Substring(0, 50));
        }

        [Fact]
        public void Content_WhenSet_UpdatesTitleAndTimestamp()
        {
            // Arrange
            var note = new Note();
            var originalUpdatedAt = note.UpdatedAt;
            System.Threading.Thread.Sleep(10);

            // Act
            note.Content = "New Title\nNew Body";

            // Assert
            note.Title.Should().Be("New Title");
            note.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public void Content_WithEmptyString_SetsTitleToNewNote()
        {
            // Arrange
            var note = new Note("Original");

            // Act
            note.Content = "";

            // Assert
            note.Title.Should().Be("New Note");
        }

        [Fact]
        public void Content_WithWhitespaceOnly_SetsTitleToNewNote()
        {
            // Arrange
            var note = new Note("Original");

            // Act
            note.Content = "   \n\t  ";

            // Assert
            note.Title.Should().Be("New Note");
        }

        [Fact]
        public void PropertyChanged_IsRaised_WhenContentChanges()
        {
            // Arrange
            var note = new Note();
            var propertyChangedRaised = false;
            note.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Note.Content))
                    propertyChangedRaised = true;
            };

            // Act
            note.Content = "New Content";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void PropertyChanged_IsRaised_WhenColorChanges()
        {
            // Arrange
            var note = new Note();
            var propertyChangedRaised = false;
            note.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Note.Color))
                    propertyChangedRaised = true;
            };

            // Act
            note.Color = "#FF0000";

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }
    }
}