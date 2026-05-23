# Boykisser Executor

A .NET 8.0 WPF Roblox Lua executor UI with Monaco editor, Velocity API integration.

## Features

- Monaco code editor with Lua syntax highlighting and autocomplete
- Script tab management (new, open, save, save as)
- Velocity API inject/execute integration
- Dark theme (GitHub Dark-inspired)
- Output panel with color-coded logs
- Self-contained Windows executable

---

## Windows

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (preinstalled on Windows 11)
- [Velocity Executor](https://velocity.com.de/) (for `VelocityAPI.dll`)
- Windows 10/11 64-bit

### Setup

1. Delete `VelocityAPI.cs` (stub)
2. Get the real `VelocityAPI.dll` from Velocity Executor
3. Add a reference in `Executor.Wpf.csproj`:
   ```xml
   <Reference Include="VelocityAPI">
     <HintPath>path\to\VelocityAPI.dll</HintPath>
   </Reference>
   ```
4. Build:
   ```cmd
   dotnet build -c Release --runtime win-x64
   ```
5. Publish:
   ```cmd
   dotnet publish -c Release --runtime win-x64
   ```
6. Run `bin\Release\net8.0-windows\win-x64\publish\BoykisserExecutor.exe` as **Administrator**

### Usage

1. Launch Roblox and join a game
2. Open BoykisserExecutor.exe as Administrator
3. Click **Inject** — attaches to Roblox
4. Type/paste a Lua script
5. Click **Execute** — runs the script

---

## Linux (cross-compile)

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Build

```bash
cd ExecutorBoykisser
dotnet build -c Release --runtime win-x64
```

### Publish

```bash
dotnet publish -c Release --runtime win-x64
```

Output: `bin/Release/net8.0-windows/win-x64/publish/BoykisserExecutor.exe`

> Cross-compilation requires `<EnableWindowsTargeting>true</EnableWindowsTargeting>` in `.csproj` (already configured).

### Transfer to Windows

Copy the `publish/` folder to a Windows machine. The `.exe` is self-contained (no .NET runtime needed).

---

## Toolbar

| Button     | Action                                    |
|------------|-------------------------------------------|
| ⚡ Inject   | Inject into / uninject from Roblox       |
| ▶ Execute  | Run current script in Roblox             |
| ⌫ Clear    | Clear the editor                         |
| 📂 Open    | Open a .lua file                         |
| 💾 Save    | Save current script                      |
| Save As    | Save to a new file                       |

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

- The stub `VelocityAPI.cs` provides empty implementations — replace it with the real `VelocityAPI.dll` for actual injection/execution
- Use a secondary Roblox account for safety
