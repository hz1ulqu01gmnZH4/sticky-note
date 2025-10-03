# Super Duper Sticky Notes

Modern sticky notes app for Windows with rich text, tags, and full-text search.

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple) ![WPF](https://img.shields.io/badge/platform-Windows-lightgrey)

## Features

- Rich text editing (Bold, Italic, Underline, Lists, Font Size)
- Tags system with visual chips
- Full-text search (FTS4)
- Auto-save (2s debounce)
- 8 color themes
- Global hotkeys: `Ctrl+Space`, `Ctrl+Shift+N`, `Ctrl+Shift+F`
- Export/Import (JSON, Markdown, CSV)
- System tray integration
- Command palette

## Quick Start

**Requirements:** Windows 10/11, .NET 8.0

```bash
git clone https://github.com/hz1ulqu01gmnZH4/sticky-note.git
cd sticky-note
dotnet build
dotnet run --project SuperDuperStickyNotes/SuperDuperStickyNotes.csproj
```

**Visual Studio:** Open `SuperDuperStickyNotes.sln` and press F5

## Usage

- **Create note:** `Ctrl+Shift+N` or right-click tray icon
- **Command palette:** `Ctrl+Space`
  - Type text → create note
  - `/search term` → search notes
  - `#show` / `#hide` → show/hide all
- **Format text:** Select text, then `Ctrl+B/I/U` or use toolbar
- **Add tags:** Click `+` button at bottom of note

## Testing

```bash
dotnet test  # 15 tests (8 model + 7 database)
```

See [TESTING_FROM_WSL.md](TESTING_FROM_WSL.md) for WSL testing.

## Tech Stack

.NET 8.0 • WPF • SQLite (FTS4) • xUnit • Newtonsoft.Json

## Development

**v1.1.0** - 85% complete • See [TASKS.md](TASKS.md) for roadmap

**Database:** `%APPDATA%/SuperDuperStickyNotes/notes.db`

---

Built with 🤖 [Claude Code](https://claude.com/claude-code)
