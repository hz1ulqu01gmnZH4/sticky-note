# Testing Guide - Super Duper Sticky Notes (C# WPF)

## Overview

This guide will help you build and test the C# WPF implementation of Super Duper Sticky Notes on Windows.

## Prerequisites

- **Windows 10/11** (WPF requires Windows)
- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** (recommended) or VS Code with C# extension

## Building the Application

### Option 1: Using dotnet CLI (Recommended)

```bash
# Navigate to the solution directory
cd SuperDuperStickyNotes

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project SuperDuperStickyNotes\SuperDuperStickyNotes.csproj
```

### Option 2: Using Visual Studio

1. Open `SuperDuperStickyNotes.sln` in Visual Studio 2022
2. Press `F6` to build the solution
3. Press `F5` to run with debugging (or `Ctrl+F5` to run without debugging)

## Running Tests

```bash
# Run all unit tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Priority 1 Features Implemented ✅

### 1. **Rich Text Editing**
- **Feature**: RichTextBox with spell check
- **How to Test**:
  1. Create a new note (Tray → New Note or `Ctrl+Shift+N`)
  2. Type some text - spell check will underline misspelled words
  3. Text is auto-saved after 2 seconds of inactivity
  4. The RichTextBox supports basic formatting (more formatting options to be added)

### 2. **Additional Global Hotkeys**
- **Features**:
  - `Ctrl+Space` - Toggle command palette
  - `Ctrl+Shift+N` - Create new note
  - `Ctrl+Shift+F` - Open command palette in search mode

- **How to Test**:
  1. Press `Ctrl+Shift+N` → New note window should appear
  2. Press `Ctrl+Shift+F` → Command palette should open with "/" already typed
  3. Press `Ctrl+Space` → Command palette should toggle on/off

### 3. **Tags Support**
- **Feature**: Tags property added to Note model
- **Status**: Model ready, UI implementation in progress
- **Database**: Tags stored as JSON array in notes table

### 4. **Settings Dialog**
- **Feature**: Comprehensive settings window with multiple tabs
- **How to Test**:
  1. Right-click system tray icon
  2. Select "Settings..."
  3. Explore tabs:
     - **General**: Startup and behavior options
     - **Appearance**: Default colors, fonts, note size
     - **Shortcuts**: View configured hotkeys
     - **Export/Import**: Export and import functionality
     - **About**: Application information

### 5. **Export Functionality**
- **Formats**: JSON, Markdown, CSV
- **How to Test**:
  1. Create several notes with content
  2. Open Settings → Export/Import tab
  3. Click "Export as JSON" → Save file
  4. Click "Export as Markdown" → Save file
  5. Click "Export as CSV" → Save file
  6. Verify exported files contain your notes

**Export Formats**:
- **JSON**: Full note data including metadata
- **Markdown**: Human-readable format with headers
- **CSV**: Spreadsheet-compatible format

### 6. **Import Functionality**
- **Format**: JSON only (others to be added)
- **How to Test**:
  1. Export notes as JSON
  2. Close application
  3. Delete/modify database
  4. Open Settings → Export/Import
  5. Click "Import from JSON"
  6. Select previously exported JSON file
  7. Restart application to see imported notes

## Core Features (Already Working)

### System Tray Integration
- **How to Test**:
  1. Application starts minimized to tray
  2. Right-click tray icon to see menu:
     - New Note
     - Show All Notes
     - Hide All Notes
     - Settings...
     - Exit
  3. Double-click tray icon → Creates new note

### Command Palette
- **How to Test**:
  1. Press `Ctrl+Space` to open
  2. Type commands:
     - `new Meeting notes` → Creates note with content
     - `/meeting` → Searches for "meeting"
     - `show` → Shows all notes
     - `hide` → Hides all notes
     - `!quit` → Exits application

### Note Windows
- **Features**:
  - Drag to move (click header)
  - Resize from bottom-right grip
  - Pin button (📌) - keeps note on top
  - Color button (•) - cycles through 8 colors
  - Close button (✕) - deletes note with confirmation
  - Auto-save after 2 seconds

- **How to Test**:
  1. Create note
  2. Type content → Should see title update
  3. Drag header → Note moves
  4. Resize from grip → Note resizes
  5. Click pin → Note stays on top
  6. Click color → Cycles through colors
  7. Click close → Asks for confirmation if has content

### Database Persistence
- **Location**: `%AppData%\SuperDuperStickyNotes\notes.db`
- **How to Test**:
  1. Create several notes
  2. Close application
  3. Reopen application
  4. Create new note → Previous notes are in database (open via Settings → Import or manually)

### Search
- **How to Test**:
  1. Create notes with different content
  2. Press `Ctrl+Space`
  3. Type `/` followed by search term
  4. Should see matching notes in suggestions

## Known Limitations

1. **Tags UI**: Tags property exists in model but UI for adding/editing tags not yet implemented
2. **Rich Text Formatting**: RichTextBox supports formatting but toolbar/hotkeys for Bold/Italic/etc not yet added
3. **Note persistence on startup**: Notes don't automatically open windows on startup (use command palette or import/export)
4. **Database**: Using FTS4 instead of FTS5 (System.Data.SQLite limitation)

## Troubleshooting

### Build Errors

**Error**: "WindowsDesktop SDK not found"
- **Solution**: Make sure you're building on Windows, not WSL/Linux

**Error**: "NuGet packages not found"
```bash
dotnet restore
dotnet clean
dotnet build
```

### Runtime Errors

**Error**: "Database locked"
- **Solution**: Close all instances of the application

**Error**: "Hotkeys not working"
- **Solution**: Make sure no other application is using the same hotkeys

### Database Issues

**Clear database**:
```
1. Close application
2. Navigate to: %AppData%\SuperDuperStickyNotes\
3. Delete notes.db file
4. Restart application
```

**View database**:
```
1. Install SQLite Browser (https://sqlitebrowser.org/)
2. Open %AppData%\SuperDuperStickyNotes\notes.db
3. View tables and data
```

## Testing Checklist

- [ ] Application builds without errors
- [ ] Application starts and shows tray icon
- [ ] Ctrl+Shift+N creates new note
- [ ] Ctrl+Shift+F opens search
- [ ] Ctrl+Space toggles command palette
- [ ] Can create note from command palette
- [ ] Can search notes with `/` prefix
- [ ] Can drag notes around
- [ ] Can resize notes
- [ ] Can change note colors
- [ ] Can pin/unpin notes
- [ ] Notes auto-save after 2 seconds
- [ ] Spell check works in notes
- [ ] Settings dialog opens from tray
- [ ] Can export to JSON
- [ ] Can export to Markdown
- [ ] Can export to CSV
- [ ] Can import from JSON
- [ ] System tray menu works
- [ ] Double-click tray creates note
- [ ] Unit tests pass

## Performance Expectations

- **Startup**: < 2 seconds
- **Note creation**: < 200ms
- **Search (100 notes)**: < 100ms
- **Auto-save**: 2 second delay
- **Memory**: ~50MB + ~2MB per note window

## Next Steps (Priority 2 Features)

After testing Priority 1 features, these are next:

1. **Tags UI** - Add tag chips to note windows
2. **Workspace switching** - Multiple workspace support
3. **Group operations** - #arrange, #stack, #spread
4. **Note templates** - Predefined note layouts
5. **Formatting toolbar** - Bold, Italic, Lists for RichTextBox

## Feedback

If you encounter issues:
1. Check the Known Limitations section
2. Try the Troubleshooting steps
3. Check the unit tests: `dotnet test`
4. Review TASKS.md for implementation status

## Build Artifacts

After building, executables are located in:
```
SuperDuperStickyNotes\bin\Debug\net8.0-windows\
SuperDuperStickyNotes.exe
```

For Release build:
```bash
dotnet build -c Release
# Output: SuperDuperStickyNotes\bin\Release\net8.0-windows\
```

For portable/published version:
```bash
dotnet publish -c Release -r win-x64 --self-contained
# Output: SuperDuperStickyNotes\bin\Release\net8.0-windows\win-x64\publish\
```

---

**Version**: 1.1.0
**Last Updated**: 2025-10-02
**Status**: Priority 1 features implemented and ready for testing
