# Boykisser Executor

A .NET 8.0 WPF Roblox Lua executor UI with Monaco editor and DLL injection via named pipe IPC.

## Features

- Monaco code editor with Lua syntax highlighting and autocomplete
- Script tab management (new, open, save, save as)
- DLL injection + named pipe IPC for script execution
- Dark theme (GitHub Dark-inspired)
- Output panel with color-coded logs
- Self-contained Windows executable

---

## Windows

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (preinstalled on Windows 11)
- Your own executor DLL (to inject into Roblox)
- Windows 10/11 64-bit

### Build

```cmd
dotnet build -c Release --runtime win-x64
```

### Publish

```cmd
dotnet publish -c Release --runtime win-x64
```

Output: `bin\Release\net8.0-windows\win-x64\publish\BoykisserExecutor.exe`

### Usage

1. Launch Roblox and join a game
2. Run `BoykisserExecutor.exe` as **Administrator**
3. Click **Inject** — pick your executor `.dll` file
4. Type/paste a Lua script in the editor
5. Click **Execute** — sends script via named pipe (`\\.\pipe\BoykisserExecutor`) to your injected DLL

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
├── MainWindow.xaml.cs      — all logic (Monaco, injector, files)
├── Injector.cs             — DLL injection + named pipe IPC
├── ExecutorDLL/            — C++ DLL template (open in VS2022)
│   ├── ExecutorDLL.sln
│   ├── ExecutorDLL.vcxproj
│   └── dllmain.cpp         — pipe listener + your Lua logic placeholder
├── AssemblyInfo.cs         — WPF theme info
├── .gitignore
├── README.md
└── Web/
    └── monaco.html         — Monaco editor HTML (embedded resource)
```

## Building Your Executor DLL

Open `ExecutorDLL/ExecutorDLL.sln` in Visual Studio 2022, build for **x64**, and inject the resulting `.dll` via the C# app.

The template:
- Opens a console in the target process for debugging
- Connects to `\\.\pipe\BoykisserExecutor`
- Listens for scripts from the C# app
- Calls `ExecuteLua()` — **you fill in the Roblox Lua execution logic**

## Notes

- Your executor DLL must connect to the named pipe `\\.\pipe\BoykisserExecutor` to receive scripts
- Windows Defender may flag the injector — it uses `CreateRemoteThread` which is a normal injection technique
- Use a secondary Roblox account for safety
