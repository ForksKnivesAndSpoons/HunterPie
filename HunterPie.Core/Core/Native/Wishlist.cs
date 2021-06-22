using HunterPie.Memory;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;
using HunterPie.Native.Game.Wishlist;
using System.Threading.Tasks;
using HunterPie.Core.Definitions;

namespace HunterPie.Core.Native
{
    public class Wishlist
    {
        private static int[] WishListOffset = { 0xe7450 };
        /// <summary>
        /// Sends a wishlist materials query to Monster Hunter: World
        /// </summary>
        /// <param name="index">position in the wishlist this query is about (0 indexed)</param>
        public static async Task QueryMaterials(int index)
        {
            var pkt = new C_GetWishlistMaterials
            {
                header = new Header { opcode = OPCODE.GetWishlistMaterials, version = 1 },
                index = index,
            };
            // TODO: This should return the response. Maybe attach a request id and return when that response arrives
            await Client.ToServer(pkt);
        }

        /// <summary>
        /// Returns wishlist array from game's memory
        /// </summary>
        public static sWishlistEntry[] GetWishList()
        {
            // TODO: should probably make a helper function in Utils or something
            var saveBase = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("LEVEL_OFFSET"), Address.GetOffsets("LevelOffsets"));
            var slot = Kernel.Read<uint>(saveBase - 8);
            var wishlist = Kernel.ReadMultilevelPtr(saveBase + (slot * 0x27E9F0), WishListOffset);
            return Kernel.ReadStructure<sWishlistEntry>(wishlist, 60);
        }
    }
}
