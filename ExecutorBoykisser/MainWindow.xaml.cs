using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using VelocityAPI;

namespace BoykisserExecutor;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ScriptTab> _tabs = new()
    {
        new ScriptTab("Untitled.lua", "-- Welcome to Boykisser Executor\n-- Powered by Velocity API\n\nprint(\"Hello, Roblox!\")"),
    };

    private int _activeIndex;
    private string? _currentFilePath;
    public VelAPI Velocity = new();
    private bool _isInjected;
    private bool _webViewInitialized;
    private readonly ObservableCollection<OutputLine> _outputLines = new();

    public MainWindow()
    {
        InitializeComponent();
        OutputList.ItemsSource = _outputLines;
        TabList.ItemsSource = _tabs;

        Loaded += async (_, _) =>
        {
            await InitializeMonaco();
            RefreshTabList();
            UpdateTitle();
            Log("AI Executor v2.0 — Velocity API Edition", "#58a6ff");
            Log("Ready. Inject into Roblox to get started.", "#58a6ff");
        };
    }

    private async Task InitializeMonaco()
    {
        var env = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(Path.GetTempPath(), "BoykisserExecutor_WebView"));
        await MonacoView.EnsureCoreWebView2Async(env);
        MonacoView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        MonacoView.CoreWebView2.Settings.IsScriptEnabled = true;
        MonacoView.CoreWebView2.Settings.IsWebMessageEnabled = true;

        var assembly = typeof(MainWindow).Assembly;
        var name = assembly.GetName().Name + ".Web.monaco.html";
        using var stream = assembly.GetManifestResourceStream(name);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            MonacoView.NavigateToString(await reader.ReadToEndAsync());
        }

        _webViewInitialized = true;
    }

    private async Task<string> GetEditorText()
    {
        if (!_webViewInitialized) return _tabs[_activeIndex].Code;
        try
        {
            return await MonacoView.CoreWebView2.ExecuteScriptAsync("window.getCode()");
        }
        catch
        {
            return _tabs[_activeIndex].Code;
        }
    }

    private async Task SetEditorText(string code)
    {
        if (!_webViewInitialized)
        {
            _tabs[_activeIndex].Code = code;
            return;
        }
        try
        {
            var escaped = JsonConvert.SerializeObject(code);
            await MonacoView.CoreWebView2.ExecuteScriptAsync($"window.setCode({escaped})");
        }
        catch { }
    }

    private void RefreshTabList() => TabList.Items.Refresh();

    private void UpdateTitle()
    {
        var t = _tabs[_activeIndex];
        Title = $"{(t.Dirty ? "\u25cf " : "")}{t.Name} - Boykisser Executor";
    }

    private void TabList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabList.SelectedIndex < 0) return;
        SaveActiveTab();
        _activeIndex = TabList.SelectedIndex;
        _ = LoadActiveTab();
    }

    private void SaveActiveTab()
    {
        if (!_webViewInitialized) return;
        try
        {
            _tabs[_activeIndex].Code = MonacoView.CoreWebView2.ExecuteScriptAsync("window.getCode()").Result;
        }
        catch { }
    }

    private async Task LoadActiveTab()
    {
        await SetEditorText(_tabs[_activeIndex].Code);
        _currentFilePath = _tabs[_activeIndex].FilePath;
        UpdateTitle();
    }

    private void NewScript_Click(object sender, RoutedEventArgs e)
    {
        SaveActiveTab();
        var name = $"Script_{_tabs.Count + 1}.lua";
        _tabs.Add(new ScriptTab(name, "-- New script\n"));
        _activeIndex = _tabs.Count - 1;
        TabList.SelectedIndex = _activeIndex;
        _currentFilePath = null;
        _ = SetEditorText(_tabs[_activeIndex].Code);
        UpdateTitle();
        Log($"Created {name}", "#3fb950");
    }

    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var content = await File.ReadAllTextAsync(dialog.FileName);
        SaveActiveTab();
        _tabs.Add(new ScriptTab(Path.GetFileName(dialog.FileName), content)
        {
            FilePath = dialog.FileName,
        });
        _activeIndex = _tabs.Count - 1;
        TabList.SelectedIndex = _activeIndex;
        _currentFilePath = dialog.FileName;
        await SetEditorText(content);
        UpdateTitle();
        Log($"Opened {dialog.FileName}", "#3fb950");
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var t = _tabs[_activeIndex];
        t.Code = await GetEditorText();

        if (string.IsNullOrEmpty(t.FilePath))
        {
            SaveAs_Click(sender, e);
            return;
        }

        await File.WriteAllTextAsync(t.FilePath, t.Code);
        t.Dirty = false;
        _currentFilePath = t.FilePath;
        RefreshTabList();
        UpdateTitle();
        Log($"Saved {t.FilePath}", "#3fb950");
    }

    private async void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            FileName = _tabs[_activeIndex].Name,
            Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        var code = await GetEditorText();
        await File.WriteAllTextAsync(dialog.FileName, code);
        var t = _tabs[_activeIndex];
        t.Code = code;
        t.Name = Path.GetFileName(dialog.FileName);
        t.FilePath = dialog.FileName;
        t.Dirty = false;
        _currentFilePath = dialog.FileName;
        RefreshTabList();
        UpdateTitle();
        Log($"Saved {dialog.FileName}", "#3fb950");
    }

    private async void Inject_Click(object sender, RoutedEventArgs e)
    {
        if (_isInjected)
        {
            Velocity.StopCommunication();
            _isInjected = false;
            UpdateInjectButton();
            Log("Uninjected from Roblox", "#d29922");
            return;
        }

        try
        {
            Velocity.StartCommunication();
            var pids = System.Diagnostics.Process.GetProcessesByName("RobloxPlayerBeta");
            if (pids.Length == 0)
                pids = System.Diagnostics.Process.GetProcessesByName("Roblox");

            if (pids.Length == 0)
            {
                Log("Roblox process not found. Start Roblox first.", "#f85149");
                return;
            }

            await Velocity.Attach(pids[0].Id);
            _isInjected = true;
            UpdateInjectButton();
            Log($"Injected into Roblox (PID: {pids[0].Id})", "#3fb950");
        }
        catch (Exception ex)
        {
            Log($"Inject failed: {ex.Message}", "#f85149");
        }
    }

    private async void Execute_Click(object sender, RoutedEventArgs e)
    {
        if (!_isInjected)
        {
            Log("Not injected! Click Inject first.", "#f85149");
            return;
        }

        var code = await GetEditorText();
        if (string.IsNullOrWhiteSpace(code))
        {
            Log("Script is empty", "#d29922");
            return;
        }

        Log($"Executing script ({code.Length} chars)...", "#58a6ff");
        _tabs[_activeIndex].Code = code;

        try
        {
            var result = Velocity.Execute(code);
            Log(result == VelocityStates.Executed
                ? "Script executed successfully"
                : $"Execute returned: {result}", "#3fb950");
        }
        catch (Exception ex)
        {
            Log($"Execute failed: {ex.Message}", "#f85149");
        }
    }

    private void UpdateInjectButton()
    {
        InjectBtn.Content = _isInjected ? "\u26a1 Injected" : "\u26a1 Inject";
        InjectBtn.Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80));
    }

    private async void Clear_Click(object sender, RoutedEventArgs e)
    {
        await SetEditorText("");
        Log("Editor cleared", "#58a6ff");
    }

    public void Log(string message, string color = "#8b949e")
    {
        Dispatcher.Invoke(() =>
        {
            var time = DateTime.Now.ToLongTimeString();
            _outputLines.Add(new OutputLine($"[{time}] {message}", ParseColor(color)));
            if (_outputLines.Count > 500)
                _outputLines.RemoveAt(0);
            OutputList.ScrollIntoView(_outputLines[^1]);
            StatusText.Text = message.Length > 80 ? message[..80] + "..." : message;
        });
    }

    private static SolidColorBrush ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        return new SolidColorBrush(Color.FromRgb(
            Convert.ToByte(hex[..2], 16),
            Convert.ToByte(hex[2..4], 16),
            Convert.ToByte(hex[4..6], 16)));
    }

    private void ClearOutput_Click(object sender, RoutedEventArgs e) => _outputLines.Clear();
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();


}

public class ScriptTab
{
    public string Name { get; set; }
    public string Code { get; set; }
    public bool Dirty { get; set; }
    public string? FilePath { get; set; }
    public string Display => Dirty ? $"\u25cf {Name}" : Name;

    public ScriptTab(string name, string code)
    {
        Name = name;
        Code = code;
    }
}

public class OutputLine
{
    public string Text { get; }
    public SolidColorBrush Color { get; }
    public OutputLine(string text, SolidColorBrush color)
    {
        Text = text;
        Color = color;
    }
}
