#define UNICODE
#define _UNICODE
#include <windows.h>
#include <string>
#include <thread>

HANDLE g_pipe = INVALID_HANDLE_VALUE;
bool g_running = false;

void OpenConsole()
{
    AllocConsole();
    SetConsoleTitleW(L"Boykisser Executor DLL");
    FILE* f;
    freopen("CONOUT$", "w", stdout);
    freopen("CONIN$", "r", stdin);
}

void ExecuteLua(const std::string& script)
{
    // TODO: Replace with your Lua execution logic
    // This is called when a script arrives from the C# app via named pipe.
}

void PipeListener()
{
    while (g_running)
    {
        g_pipe = CreateFileW(
            L"\\\\.\\pipe\\BoykisserExecutor",
            GENERIC_READ | GENERIC_WRITE,
            0, NULL, OPEN_EXISTING, 0, NULL);

        if (g_pipe == INVALID_HANDLE_VALUE)
        {
            Sleep(100);
            continue;
        }

        char buffer[65536];
        DWORD bytesRead;

        while (g_running)
        {
            if (ReadFile(g_pipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL))
            {
                buffer[bytesRead] = '\0';
                std::string script(buffer, bytesRead);
                ExecuteLua(script);
            }
            else
            {
                break;
            }
        }

        CloseHandle(g_pipe);
        g_pipe = INVALID_HANDLE_VALUE;
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID lpReserved)
{
    switch (reason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(hModule);
        OpenConsole();
        g_running = true;
        std::thread(PipeListener).detach();
        break;
    case DLL_PROCESS_DETACH:
        g_running = false;
        if (g_pipe != INVALID_HANDLE_VALUE)
            CloseHandle(g_pipe);
        break;
    }
    return TRUE;
}
