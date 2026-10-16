using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class LookingForGuildMatch : NetPacket
	{
		public LookingForGuildMatch(short mode, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_SEARCHING_PARTY_START_ACK);
			ns.Write(mode);
			ns.Write((byte)1);
			_ = last;
		}
	}
}
