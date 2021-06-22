using HunterPie.Memory;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;
using HunterPie.Native.Game.Wishlist;
using System.Threading.Tasks;
using HunterPie.Core.Definitions;
using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HunterPie.Core.Native
{
    public class Wishlist
    {
        private static readonly SemaphoreSlim locke = new(1, 1);
        private static bool subscribed = false;
        private static Dictionary<long, S_GetWishlistMaterials> responses = new();
        private static int[] WishListOffset = { 0xe7450 };
        /// <summary>
        /// Sends a wishlist materials query to Monster Hunter: World
        /// </summary>
        /// <param name="index">position in the wishlist this query is about (0 indexed)</param>
        public static async Task<sItem[]> QueryMaterials(int index)
        {
            long id = 0;
            try
            {
                await locke.WaitAsync();
                var pkt = new C_GetWishlistMaterials
                {
                    header = new Header { opcode = OPCODE.GetWishlistMaterials, version = 1 },
                    id = DateTime.Now.Ticks,
                    index = index,
                };
                if (!subscribed)
                {
                    PacketHandlers.Handlers
                        .Where(i => i.GetType() == typeof(WishlistHandler))
                        .Select(i => (WishlistHandler)i)
                        .First()
                        .OnReceiveMaterials += QueueResponses;
                    subscribed = true;
                }
                id = pkt.id;
                await Client.ToServer(pkt);
            }
            finally
            {
                locke.Release();
            }

            bool responded = false;
            S_GetWishlistMaterials response = new();
            while (!responded)
            {
                try
                {
                    await locke.WaitAsync();
                    responded = responses.TryGetValue(id, out response);
                    responses.Remove(id);
                }
                finally
                {
                    locke.Release();
                }
            }
            return response.materials;
        }

        private static async void QueueResponses(object sender, S_GetWishlistMaterials e)
        {
            try
            {
                await locke.WaitAsync();
                responses.Add(e.id, e);
            }
            finally
            {
                locke.Release();
            }
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
