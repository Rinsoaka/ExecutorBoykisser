#include <windows.h>
#include <iostream>
#include <string>
#include <thread>
#include <vector>

HANDLE g_pipe = INVALID_HANDLE_VALUE;
bool g_running = false;

void OpenConsole()
{
    AllocConsole();
    SetConsoleTitle(L"Boykisser Executor DLL");
    FILE* f;
    freopen_s(&f, "CONOUT$", "w", stdout);
    freopen_s(&f, "CONIN$", "r", stdin);
    std::cout << "[Executor DLL] Loaded into target process." << std::endl;
}

void ExecuteLua(const std::string& script)
{
    // TODO: Replace with your Lua execution logic
    // This is where you call Roblox's internal Lua VM
    //
    // Example approaches:
    // 1. Find rbx::lua::State or similar in Roblox's binary
    // 2. Write bytecode to a Roblox core module
    // 3. Use a custom Lua interpreter embedded in the DLL
    //
    // std::cout << "[Executor DLL] Executing: " << script << std::endl;
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

        std::cout << "[Executor DLL] Connected to pipe." << std::endl;

        char buffer[65536];
        DWORD bytesRead;

        while (g_running)
        {
            if (ReadFile(g_pipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL))
            {
                buffer[bytesRead] = '\0';
                std::string script(buffer, bytesRead);
                std::cout << "[Executor DLL] Received script (" << script.size() << " chars)" << std::endl;
                ExecuteLua(script);
            }
            else
            {
                break;
            }
        }

        CloseHandle(g_pipe);
        g_pipe = INVALID_HANDLE_VALUE;
        std::cout << "[Executor DLL] Pipe disconnected." << std::endl;
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
