# Super Duper Sticky Notes

A modern, feature-rich sticky notes application for Windows built with C# and WPF.

![Version](https://img.shields.io/badge/version-1.1.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ Features

### Core Functionality
- 📝 **Rich Text Editing** with spell check
  - Bold, Italic, Underline formatting (Ctrl+B/I/U)
  - Bullet and numbered lists
  - Adjustable font sizes (10-20pt)
- 🎨 **8 Color Themes** - Yellow, Blue, Green, Pink, Purple, Orange, Red, Gray
- 🏷️ **Tags System** - Organize notes with multiple tags
- 🔍 **Full-Text Search** - Find notes instantly with FTS4
- 💾 **Auto-save** - 2-second debounce, never lose your work
- 📌 **Pin Notes** - Keep important notes always on top

### Window Management
- 🪟 Individual floating note windows
- 📐 Resizable with drag handles
- 💫 Remember position and size
- 🎯 System tray integration

### Advanced Features
- ⌨️ **Global Hotkeys**
  - `Ctrl+Space` - Toggle command palette
  - `Ctrl+Shift+N` - Create new note
  - `Ctrl+Shift+F` - Quick search
- 📤 **Export/Import**
  - JSON (full data)
  - Markdown (human-readable)
  - CSV (spreadsheet)
- ⚙️ **Settings Dialog** - Customize appearance, shortcuts, and more
- 🎨 **Command Palette** - Quick access to all features

## 🚀 Quick Start

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK or Runtime

### Installation

1. Clone the repository:
```bash
git clone https://github.com/hz1ulqu01gmnZH4/sticky-note.git
cd sticky-note
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run --project SuperDuperStickyNotes/SuperDuperStickyNotes.csproj
```

### From Visual Studio
1. Open `SuperDuperStickyNotes.sln`
2. Press `F5` to build and run

## 📖 Usage

### Creating Notes
- Right-click system tray icon → **New Note**
- Press `Ctrl+Shift+N` (global hotkey)
- Use command palette: `Ctrl+Space` then type text

### Command Palette
Press `Ctrl+Space` to open the command palette:

**Commands:**
- Type text directly to create a new note
- `/search term` - Search notes
- `#show` - Show all notes
- `#hide` - Hide all notes
- `!minimize` - Minimize to tray
- `!quit` - Exit application

### Formatting Text
1. Select text in a note
2. Use toolbar buttons or keyboard shortcuts:
   - **Bold**: Ctrl+B
   - **Italic**: Ctrl+I
   - **Underline**: Ctrl+U
   - Click bullet/numbered list buttons
   - Select font size from dropdown

### Managing Tags
1. Click the **+** button at bottom of note
2. Enter tag name and press OK
3. Click **✕** on any tag to remove it
4. Tags are automatically saved

## 🧪 Testing

### Running Tests
```bash
dotnet test
```

All 15 unit tests should pass:
- 8 Note model tests
- 7 DatabaseContext tests

### Testing from WSL
See [TESTING_FROM_WSL.md](TESTING_FROM_WSL.md) for instructions on running tests from Windows Subsystem for Linux.

## 📁 Project Structure

```
SuperDuperStickyNotes/
├── SuperDuperStickyNotes/          # Main application
│   ├── Data/                       # Database context
│   ├── Models/                     # Data models
│   ├── Services/                   # Business logic
│   ├── Windows/                    # Window views
│   └── Styles/                     # XAML styles
├── SuperDuperStickyNotes.Tests/    # Unit tests
├── TASKS.md                        # Development roadmap
├── TESTING_GUIDE.md                # Comprehensive testing guide
└── TESTING_FROM_WSL.md             # WSL testing instructions
```

## 🗄️ Database

Notes are stored in SQLite database at:
```
%APPDATA%/SuperDuperStickyNotes/notes.db
```

Features:
- Full-text search with FTS4
- Automatic migrations
- Tags stored as JSON arrays
- Position/size persistence

## 🛠️ Technology Stack

- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Database**: SQLite with System.Data.SQLite
- **Testing**: xUnit, Moq, FluentAssertions
- **Serialization**: Newtonsoft.Json

## 📋 Development Status

**Version 1.1.0** - Priority 1 Features (85% Complete)

✅ **Completed:**
- RichTextBox with spell check
- Formatting toolbar (Bold, Italic, Underline, Lists, Font Size)
- Tags UI (Add/Remove, visual chips)
- Settings dialog
- Export/Import (JSON, Markdown, CSV)
- Global hotkeys
- Unit tests (15 tests passing)

🚧 **In Progress:**
- Settings persistence
- Note templates
- Automatic backups

See [TASKS.md](TASKS.md) for detailed roadmap.

## 🐛 Known Issues

- FTS4 used instead of FTS5 (System.Data.SQLite limitation)
- Settings dialog values not yet persisted
- List formatting could be more sophisticated

## 📝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Inspired by the Rust sticky notes application
- Built with ❤️ using C# and WPF

## 📞 Support

For issues, questions, or suggestions:
- Create an issue on GitHub
- See [TESTING_GUIDE.md](TESTING_GUIDE.md) for troubleshooting

---

**Made with** 🤖 **[Claude Code](https://claude.com/claude-code)**
