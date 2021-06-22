#pragma once

#include "../../Connection/Packets/model.h"

namespace Game
{
    namespace Wishlist
    {
        struct sItem
        {
            unsigned long long unk0;
            int ItemId;
            int Amount;
        };

        struct WishlistEntry {
            unsigned long long _class;
            unsigned int category;
            unsigned int type;
            unsigned int equipId;
            unsigned int pendantId;
        };

        struct C_GetWishlistMaterials : Connection::Packets::I_PACKET
        {
            long long id;
            int index;
        };

        struct S_GetWishlistMaterials : Connection::Packets::I_PACKET
        {
            long long id;
            sItem materials[4];
        };
    }
}
