using System.Collections.Generic;

namespace HunterPie.Native.Connection
{
    public sealed class PacketHandlers
    {
        private static PacketHandlers instance = null;
        private static readonly object locke = new();
        public static PacketHandlers Instance
        {
            get
            {
                lock (locke)
                {
                    return instance ?? (instance = new(new()
                    {
                        new Game.Wishlist.WishlistHandler(),
                    }));
                }
            }
        }
        private List<IPacketHandler> _handlers;
        private PacketHandlers(List<IPacketHandler> handlers)
        {
            _handlers = handlers;
        }
        public static List<IPacketHandler> Handlers
        {
            get => Instance._handlers;
        }

    }
}
