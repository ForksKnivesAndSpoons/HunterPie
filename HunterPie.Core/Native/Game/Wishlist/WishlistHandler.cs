using System;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;

namespace HunterPie.Native.Game.Wishlist
{
    class WishlistHandler : IPacketHandler
    {
        public event EventHandler<S_GetWishlistMaterials> OnReceiveMaterials;

        public bool CanHandlePackets(OPCODE opcode)
        {
            return opcode == OPCODE.GetWishlistMaterials;
        }
        public void HandlePackets(byte[] buffer)
        {
            var pkt = PacketParser.Deserialize<S_GetWishlistMaterials>(buffer);
            OnReceiveMaterials?.Invoke(this, pkt);
        }

    }
}
