# Testing from WSL (Windows Subsystem for Linux)

## Overview

Although WPF applications require Windows to run, you can execute unit tests from WSL by using the Windows version of dotnet.exe. This allows you to develop and test the C# WPF application from a Linux environment.

## Prerequisites

- **WSL** (Windows Subsystem for Linux) installed
- **.NET SDK** installed on Windows (typically in `C:\Program Files\dotnet\`)
- Project files accessible from WSL (usually in `/home/user/...` or `/mnt/c/...`)

## Running Tests from WSL

### Quick Method

```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

**Note:** Replace `Arch` with your WSL distribution name (e.g., `Ubuntu`, `Debian`, etc.)

### Step-by-Step Instructions

#### 1. Verify Windows dotnet is available

```bash
ls /mnt/c/Program\ Files/dotnet/dotnet.exe
```

Expected output:
```
/mnt/c/Program Files/dotnet/dotnet.exe
```

#### 2. Get your WSL distribution name

```bash
wsl.exe -l -v
```

Example output:
```
  NAME      STATE           VERSION
* Arch      Running         2
  Ubuntu    Stopped         2
```

#### 3. Convert your WSL path to Windows path

```bash
# Get the Windows UNC path for your project
wslpath -w /home/ak/super-duper-sticky-notes/SuperDuperStickyNotes
```

Example output:
```
\\wsl.localhost\Arch\home\ak\super-duper-sticky-notes\SuperDuperStickyNotes
```

#### 4. Run the tests

**Basic test run:**
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

**With detailed output:**
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes" --logger "console;verbosity=detailed"
```

**With longer timeout (for slow systems):**
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes" --logger "console;verbosity=normal" -- --timeout 120000
```

## Expected Output

### Successful Test Run

```
テスト実行を開始しています。お待ちください...
合計 1 個のテスト ファイルが指定されたパターンと一致しました。
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.4.1+b9eacec401 (64-bit .NET 8.0.20)
[xUnit.net 00:00:00.65]   Discovering: SuperDuperStickyNotes.Tests
[xUnit.net 00:00:00.67]   Discovered:  SuperDuperStickyNotes.Tests
[xUnit.net 00:00:00.67]   Starting:    SuperDuperStickyNotes.Tests

  成功 SuperDuperStickyNotes.Tests.Models.NoteTests.Constructor_WithoutContent_CreatesNoteWithDefaults
  成功 SuperDuperStickyNotes.Tests.Models.NoteTests.Constructor_WithContent_SetsTitleFromFirstLine
  ... (13 more tests)

テストの実行に成功しました。
テストの合計数: 15
     成功: 15
合計時間: 2.9620 秒
```

### Test Summary

The test suite includes **15 tests** across 2 categories:

#### Model Tests (8 tests)
- `Constructor_WithoutContent_CreatesNoteWithDefaults`
- `Constructor_WithContent_SetsTitleFromFirstLine`
- `Constructor_WithLongContent_TruncatesTitleAt50Chars`
- `Content_WhenSet_UpdatesTitleAndTimestamp`
- `Content_WithEmptyString_SetsTitleToNewNote`
- `Content_WithWhitespaceOnly_SetsTitleToNewNote`
- `PropertyChanged_IsRaised_WhenContentChanges`
- `PropertyChanged_IsRaised_WhenColorChanges`

#### Database Tests (7 tests)
- `CreateNote_ValidNote_ReturnsNoteWithSameId`
- `GetNote_ExistingId_ReturnsNote`
- `GetNote_NonExistentId_ReturnsNull`
- `GetAllNotes_WithMultipleNotes_ReturnsAllNotes`
- `UpdateNote_ExistingNote_UpdatesContent`
- `DeleteNote_ExistingNote_RemovesNote`
- `SearchNotes_MatchingContent_ReturnsMatchingNotes`

## Troubleshooting

### Issue: "command not found: dotnet.exe"

**Solution:** Verify Windows dotnet installation:
```bash
ls /mnt/c/Program\ Files/dotnet/
```

If not found, install .NET SDK on Windows from https://dotnet.microsoft.com/download

### Issue: "MSB1001: 不明なスイッチです"

**Cause:** The path format is incorrect (using Linux path instead of Windows UNC path)

**Solution:** Use double backslashes for UNC path:
```bash
# ❌ Wrong:
/mnt/c/Program\ Files/dotnet/dotnet.exe test /home/ak/project

# ✅ Correct:
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\project"
```

### Issue: "WindowsDesktop SDK not found"

**Cause:** This error only occurs if you try to use Linux dotnet instead of Windows dotnet

**Solution:** Make sure you're using `/mnt/c/Program\ Files/dotnet/dotnet.exe` (Windows version), not just `dotnet` (Linux version)

### Issue: Tests hang or timeout

**Solution:** Increase timeout or check if antivirus is scanning the test files:
```bash
# Add timeout parameter
/mnt/c/Program\ Files/dotnet/dotnet.exe test "..." --timeout 300000
```

### Issue: Permission denied

**Solution:** Check file permissions in WSL:
```bash
# Make sure files are readable
ls -la SuperDuperStickyNotes/

# If needed, fix permissions
chmod -R u+rw SuperDuperStickyNotes/
```

## Creating a Shell Alias

For convenience, create an alias in your `~/.bashrc` or `~/.zshrc`:

```bash
# Add to ~/.bashrc or ~/.zshrc
alias dotnet-win='/mnt/c/Program\ Files/dotnet/dotnet.exe'

# Then you can use:
dotnet-win test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

Or create a function for automatic path conversion:

```bash
# Add to ~/.bashrc or ~/.zshrc
function dotnet-test-wsl() {
    local wsl_path="$1"
    local win_path=$(wslpath -w "$wsl_path" | sed 's/\\/\\\\/g')
    /mnt/c/Program\ Files/dotnet/dotnet.exe test "$win_path"
}

# Usage:
dotnet-test-wsl /home/ak/super-duper-sticky-notes/SuperDuperStickyNotes
```

## Performance Notes

- Tests typically complete in **~3 seconds**
- First run may take longer due to NuGet restore and compilation
- Subsequent runs are faster due to caching
- Database tests use temporary in-memory databases for isolation

## Integration with CI/CD

This approach can be useful for CI/CD pipelines running on WSL or GitHub Actions with Windows runners:

```yaml
# .github/workflows/test.yml
name: Run Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Run Tests
        run: dotnet test SuperDuperStickyNotes --logger "console;verbosity=normal"
```

## Additional Commands

### Build only (without running tests)
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe build "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

### Restore packages
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe restore "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

### Clean build artifacts
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe clean "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes"
```

### Run specific test
```bash
/mnt/c/Program\ Files/dotnet/dotnet.exe test "\\\\wsl.localhost\\Arch\\home\\ak\\super-duper-sticky-notes\\SuperDuperStickyNotes" --filter "FullyQualifiedName~CreateNote_ValidNote_ReturnsNoteWithSameId"
```

## Limitations

- **Cannot run the WPF application** from WSL (GUI requires Windows)
- **Unit tests only** - integration tests requiring UI won't work
- **Build artifacts** are created in Windows-accessible paths
- **Debugging** is limited - use Visual Studio on Windows for full debugging

## Alternatives

If you need full Windows GUI testing:

1. **Use Windows directly** - Best option for GUI testing
2. **Use Remote Desktop** - Access Windows GUI from WSL
3. **Use Visual Studio 2022** - Native WPF development experience
4. **Use VS Code Remote** - Edit in WSL, run on Windows

---

**Last Updated:** 2025-10-02
**Tested On:** WSL2 (Arch), Windows 11, .NET 8.0 SDK
