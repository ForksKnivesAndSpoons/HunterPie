#pragma once

#include "WishlistTypes.h"
#include "../../Connection/IPacketHandler.h"

namespace Game
{
    namespace Wishlist
    {
        using FnGetMaterials = unsigned char (*)(long long, long long, char);

        class WishlistHandler : public Connection::IPacketHandler
        {
        private:
            long long* saveBase;
            FnGetMaterials originalGetMaterials;

            void getMaterials(int index, S_GetWishlistMaterials& response);
        public:
            void LoadAddress(uintptr_t arr[128]) override;
            bool receivePackets(Connection::Packets::I_PACKET& packet) override;
        };
    }
}
