using System.Runtime.InteropServices;
using HunterPie.Native.Connection.Packets;
using HunterPie.Core.Definitions;

namespace HunterPie.Native.Game.Wishlist
{
    [StructLayout(LayoutKind.Sequential)]
    public struct C_GetWishlistMaterials
    {
        public Header header;
        public long id;
        public int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct S_GetWishlistMaterials
    {
        public Header header;
        public long id;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public sItem[] materials;
    }
}
