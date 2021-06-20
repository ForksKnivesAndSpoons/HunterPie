#include "WishlistHandler.h"
#include "../../Connection/Packets/address_map.h"
#include "../../Connection/Packets/model.h"
#include "../../Connection/Socket.h"
#include "../Helpers.h"

namespace Game
{
    namespace Wishlist
    {
        using Connection::Packets::AddressIndex;
        using Connection::Packets::OPCODE;
        using Connection::Server;

        void WishlistHandler::getMaterials(int index, S_GetWishlistMaterials& response) {
            auto that = ((long long)&response.materials[0]) - 0x2f10;
            auto wishlist = resolvePtrs<WishlistEntry>(saveBase, { 0xa8, 0xe7450 });
            originalGetMaterials(that, (long long)&wishlist[index], 1);
        }

        void WishlistHandler::LoadAddress(uintptr_t arr[128]) {
            saveBase = (long long*)arr[AddressIndex::LEVEL_OFFSET];
            originalGetMaterials = (FnGetMaterials)arr[AddressIndex::FUN_SET_WISHLIST_MATERIALS];
        }

        bool WishlistHandler::receivePackets(Connection::Packets::I_PACKET& packet) {
            if (packet.header.opcode == OPCODE::GetWishlistMaterials) {
                auto request = reinterpret_cast<C_GetWishlistMaterials*>(&packet);
                S_GetWishlistMaterials response;
                response.header.opcode = OPCODE::GetWishlistMaterials;

                getMaterials(request->index, response);
                Server::getInstance()->sendData(&response, sizeof(response));
                return true;
            } else {
                return false;
            }
        }
    }
}
