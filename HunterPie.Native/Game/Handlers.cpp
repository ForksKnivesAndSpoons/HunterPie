#include "Handlers.h"

#include <memory>

#include "../Connection/Socket.h"
#include "Wishlist/WishlistHandler.h"

namespace Game
{
    void addPacketHandlers() {
        Connection::packetHandlers.push_back(std::make_unique<Wishlist::WishlistHandler>());
    }
}
