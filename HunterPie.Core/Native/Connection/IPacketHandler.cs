using HunterPie.Native.Connection.Packets;

namespace HunterPie.Native.Connection
{
    public interface IPacketHandler
    {
        public bool CanHandlePackets(OPCODE opcode);
        public void HandlePackets(byte[] buffer);
    }
}
