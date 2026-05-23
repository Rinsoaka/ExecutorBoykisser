using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BoykisserExecutor;

public class Injector : IDisposable
{
    private const string PipeName = "BoykisserExecutor";
    private NamedPipeServerStream? _pipeServer;
    private bool _injected;
    private bool _disposed;

    public bool IsInjected => _injected;

    public bool InjectRoblox(string dllPath, int pid)
    {
        if (_injected) return true;
        if (!File.Exists(dllPath))
            throw new FileNotFoundException("Executor DLL not found", dllPath);

        Uninject();

        using var process = System.Diagnostics.Process.GetProcessById(pid);
        var handle = process.Handle;

        var dllBytes = Encoding.Unicode.GetBytes(dllPath);
        var alloc = VirtualAllocEx(handle, IntPtr.Zero, dllBytes.Length,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ReadWrite);
        if (alloc == IntPtr.Zero)
            throw new Exception("VirtualAllocEx failed");

        if (!WriteProcessMemory(handle, alloc, dllBytes, dllBytes.Length, out _))
            throw new Exception("WriteProcessMemory failed");

        var loadLib = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
        var thread = CreateRemoteThread(handle, IntPtr.Zero, 0,
            loadLib, alloc, 0, IntPtr.Zero);
        if (thread == IntPtr.Zero)
            throw new Exception("CreateRemoteThread failed");

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                _ = WaitForPipeConnectionAsync();
                _injected = true;
                return true;
            }
            catch (IOException) when (attempt < 2)
            {
                System.Threading.Thread.Sleep(500);
            }
        }
        throw new Exception("Failed to create named pipe — all instances busy. Close any other instances and try again.");
    }

    public bool Uninject()
    {
        if (!_injected) return true;
        _pipeServer?.Dispose();
        _pipeServer = null;
        _injected = false;
        return true;
    }

    public Task<bool> ExecuteAsync(string script)
    {
        if (!_injected)
            return Task.FromResult(false);

        return Task.Run(() =>
        {
            try
            {
                if (_pipeServer?.IsConnected == true)
                {
                    var data = Encoding.UTF8.GetBytes(script);
                    _pipeServer.Write(data, 0, data.Length);
                    _pipeServer.Flush();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        });
    }

    private async Task WaitForPipeConnectionAsync()
    {
        try
        {
            await _pipeServer!.WaitForConnectionAsync();
        }
        catch { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Uninject();
        _disposed = true;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
        int dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
        byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes,
        uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter,
        uint dwCreationFlags, IntPtr lpThreadId);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [Flags]
    private enum AllocationType : uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,
    }

    [Flags]
    private enum MemoryProtection : uint
    {
        ReadWrite = 0x04,
    }
}
