using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SuperDuperStickyNotes.Models
{
    public class Note : INotifyPropertyChanged
    {
        private string _id;
        private string _content;
        private string _title;
        private string _color;
        private Position _position;
        private Size _size;
        private string _workspaceId;
        private string? _groupId;
        private bool _pinned;
        private DateTime _createdAt;
        private DateTime _updatedAt;
        private Dictionary<string, object> _metadata;
        private List<string> _tags;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                Title = GetTitleFromContent(value);
                UpdatedAt = DateTime.UtcNow;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public Position Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(); }
        }

        public Size Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(); }
        }

        public string WorkspaceId
        {
            get => _workspaceId;
            set { _workspaceId = value; OnPropertyChanged(); }
        }

        public string? GroupId
        {
            get => _groupId;
            set { _groupId = value; OnPropertyChanged(); }
        }

        public bool Pinned
        {
            get => _pinned;
            set { _pinned = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(); }
        }

        public Dictionary<string, object> Metadata
        {
            get => _metadata;
            set { _metadata = value; OnPropertyChanged(); }
        }

        public List<string> Tags
        {
            get => _tags;
            set { _tags = value; OnPropertyChanged(); }
        }

        public Note()
        {
            _id = Guid.NewGuid().ToString();
            _content = string.Empty;
            _title = "New Note";
            _color = "#FFEB3B";
            _position = new Position { X = 100, Y = 100 };
            _size = new Size { Width = 250, Height = 300 };
            _workspaceId = "default";
            _groupId = null;
            _pinned = false;
            _createdAt = DateTime.UtcNow;
            _updatedAt = DateTime.UtcNow;
            _metadata = new Dictionary<string, object>();
            _tags = new List<string>();
        }

        public Note(string content) : this()
        {
            Content = content;
        }

        private string GetTitleFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "New Note";

            var lines = content.Split('\n');
            var firstLine = lines[0].Trim();

            if (firstLine.Length > 50)
                firstLine = firstLine.Substring(0, 50);

            return string.IsNullOrWhiteSpace(firstLine) ? "New Note" : firstLine;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}