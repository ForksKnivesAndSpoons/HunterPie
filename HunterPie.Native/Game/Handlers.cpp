#include "Handlers.h"

#include "../Connection/Socket.h"

namespace Game
{
    void addPacketHandlers() {
        Connection::packetHandlers.clear();
    }
}
