#pragma once

#include <cstdint>

#include "Packets/model.h"

namespace Connection
{
    class IPacketHandler
    {
    public:
        virtual void LoadAddress(uintptr_t arr[128]) {}
        virtual void InitializeHooks() {}
        virtual bool receivePackets(Packets::I_PACKET& packet) = 0;
    };
}
