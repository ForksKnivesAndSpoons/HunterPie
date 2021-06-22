using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential, Size = 24)]
    public struct sWishlistEntry : IEquatable<sWishlistEntry>
    {
        public long vPtr;
        public uint Category;
        public uint Type;
        public uint EquipId;
        public uint PendantId;

        public bool Equals(sWishlistEntry other)
        {
            return Category == other.Category
                && Type == other.Type
                && EquipId == other.EquipId
                && PendantId == other.PendantId;
        }
    }

    public class sWishlistEntryEqualityComparer : IEqualityComparer<sWishlistEntry>
    {
        public bool Equals(sWishlistEntry a, sWishlistEntry b)
        {
            return a.Equals(b);
        }

        public int GetHashCode(sWishlistEntry obj)
        {
            return (obj.Category, obj.Type, obj.EquipId, obj.PendantId).GetHashCode();
        }
    }
}
