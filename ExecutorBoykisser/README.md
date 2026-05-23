# Boykisser Executor

A .NET 8.0 WPF Roblox Lua executor UI with Monaco editor, Velocity API integration.

## Features

- Monaco code editor with Lua syntax highlighting and autocomplete
- Script tab management (new, open, save, save as)
- Velocity API inject/execute integration
- Dark theme (GitHub Dark-inspired)
- Output panel with color-coded logs
- Self-contained Windows executable

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for building)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (installed by default on Windows 11)
- [Velocity Executor](https://velocity.com.de/) (for the real `VelocityAPI.dll`)
- Windows 10/11 64-bit (for running the app)

## Building

### On Linux (cross-compile)

```bash
cd ExecutorBoykisser
dotnet build -c Release --runtime win-x64
```

### On Windows (with real VelocityAPI.dll)

1. Delete `VelocityAPI.cs` stub
2. Add a reference to the real `VelocityAPI.dll` in `Executor.Wpf.csproj`:
   ```xml
   <Reference Include="VelocityAPI">
     <HintPath>path\to\VelocityAPI.dll</HintPath>
   </Reference>
   ```
3. Build:
   ```bash
   dotnet build -c Release --runtime win-x64
   ```

## Publishing (self-contained .exe)

```bash
dotnet publish -c Release --runtime win-x64
```

Output: `bin/Release/net8.0-windows/win-x64/publish/BoykisserExecutor.exe`

## Setup & Usage

1. **Install WebView2 Runtime** if not already present (Windows 11 has it built-in)
2. **Launch Roblox** and join any game
3. **Run BoykisserExecutor.exe** as administrator (required for injection)
4. **Click Inject** — attaches to the Roblox process
5. Type or paste a Lua script in the editor
6. **Click Execute** — runs the script in Roblox

### Toolbar

| Button     | Action                                    |
|------------|-------------------------------------------|
| ⚡ Inject   | Inject into / uninject from Roblox       |
| ▶ Execute  | Run current script in Roblox             |
| ⌫ Clear    | Clear the editor                         |
| 📂 Open    | Open a .lua file                         |
| 💾 Save    | Save current script                      |
| Save As    | Save to a new file                       |

### Script Tabs

- Click **+** in the sidebar to create a new script tab
- Click a tab to switch between scripts
- A filled circle (●) indicates unsaved changes

### Output Panel

Displays log messages with color coding:
- **Blue** — info / status
- **Green** — success
- **Red** — errors
- **Yellow** — warnings

## Project Structure

```
ExecutorBoykisser/
├── Executor.Wpf.csproj    — project file
├── App.xaml / App.xaml.cs  — application entry
├── MainWindow.xaml         — UI layout
├── MainWindow.xaml.cs      — all logic (Monaco, VelAPI, files)
├── VelocityAPI.cs          — stub (delete on Windows, use real DLL)
├── AssemblyInfo.cs         — WPF theme info
├── .gitignore
├── README.md
└── Web/
    └── monaco.html         — Monaco editor HTML (embedded resource)
```

## Notes

- WPF cross-compilation from Linux requires `<EnableWindowsTargeting>true</EnableWindowsTargeting>` in `.csproj`
- The stub `VelocityAPI.cs` provides empty implementations — replace it with the real `VelocityAPI.dll` for actual injection/execution
- Use a secondary Roblox account for safety
