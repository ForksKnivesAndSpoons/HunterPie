#pragma once
#include "Connection/Socket.h"
#include "Game/Handlers.h"

using namespace Connection;

void LoadNativeDll()
{

    std::thread([]()
    {
#if _DEBUG
        AllocConsole();
        FILE* newStdout;
        freopen_s(&newStdout, "CONOUT$", "w", stdout);
#endif
        Game::addPacketHandlers();
        Server::getInstance()->initialize();

    }).detach();

}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        LoadNativeDll();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
